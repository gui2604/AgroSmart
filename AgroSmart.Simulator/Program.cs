using System.Text.Json;
using Confluent.Kafka;

// ---------------------------------------------------------------------------
// AgroSmart - Simulador de telemetria de campo (producer Kafka)
//
// Gera um fluxo contínuo de leituras de sensores agrícolas e publica em um
// tópico Kafka. A API AgroSmart consome esse tópico e ingere as leituras
// (persistência + avaliação de regras de alerta), formando o pipeline de dados
// em tempo real exigido na Fase 4.
// ---------------------------------------------------------------------------

string bootstrap = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS") ?? "localhost:9092";
string topic = Environment.GetEnvironmentVariable("KAFKA_TOPIC") ?? "agrosmart.sensor-readings";
int intervalMs = int.TryParse(Environment.GetEnvironmentVariable("SIMULATOR_INTERVAL_MS"), out var v) ? v : 3000;

var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
var rng = new Random();

// Sensores instalados no campo (devem existir na base da API para a ingestão resolver).
var devices = new[] { "SENSOR-T1-01", "SENSOR-T2-01", "SENSOR-E1-01" };

// Faixas plausíveis por métrica. Ocasionalmente o simulador extrapola a faixa
// para que a API gere alertas e o pipeline fique visível de ponta a ponta.
var metrics = new (string Code, double Min, double Max)[]
{
    ("TEMPERATURE",   18, 30),
    ("HUMIDITY",      45, 80),
    ("SOIL_MOISTURE", 40, 65),
    ("PH",           5.6, 6.6),
    ("LUMINOSITY",   250, 750)
};

Console.WriteLine("============================================================");
Console.WriteLine(" AgroSmart - Simulador de Telemetria (Kafka Producer)");
Console.WriteLine("============================================================");
Console.WriteLine($" Brokers : {bootstrap}");
Console.WriteLine($" Topico  : {topic}");
Console.WriteLine($" Intervalo: {intervalMs} ms");
Console.WriteLine("------------------------------------------------------------");

var config = new ProducerConfig
{
    BootstrapServers = bootstrap,
    Acks = Acks.All,
    EnableIdempotence = true
};

using var producer = new ProducerBuilder<string, string>(config).Build();

using var cts = new CancellationTokenSource();
Console.CancelKeyPress += (_, e) =>
{
    e.Cancel = true;
    cts.Cancel();
    Console.WriteLine("\nEncerrando simulador...");
};

long counter = 0;
while (!cts.IsCancellationRequested)
{
    var device = devices[rng.Next(devices.Length)];

    var measurements = new List<object>();
    foreach (var m in metrics)
    {
        // ~15% das amostras saem fora da faixa para acionar regras de alerta.
        bool anomaly = rng.NextDouble() < 0.15;
        double value = anomaly
            ? RoundFor(m.Code, m.Max + rng.NextDouble() * (m.Max - m.Min) * 0.4)
            : RoundFor(m.Code, m.Min + rng.NextDouble() * (m.Max - m.Min));

        // Para umidade do solo a anomalia relevante é o valor BAIXO (estresse hídrico).
        if (anomaly && m.Code is "SOIL_MOISTURE" or "HUMIDITY")
            value = RoundFor(m.Code, m.Min - rng.NextDouble() * 15);

        measurements.Add(new { metricCode = m.Code, value });
    }

    var payload = new
    {
        deviceIdentifier = device,
        collectedAt = DateTime.UtcNow,
        measurements
    };

    string json = JsonSerializer.Serialize(payload, jsonOptions);

    try
    {
        var delivery = await producer.ProduceAsync(topic, new Message<string, string>
        {
            Key = device,
            Value = json
        }, cts.Token);

        counter++;
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] #{counter} -> {device} (partition {delivery.Partition.Value}, offset {delivery.Offset.Value})");
    }
    catch (ProduceException<string, string> ex)
    {
        Console.WriteLine($"Falha ao publicar: {ex.Error.Reason}");
        await Task.Delay(2000, cts.Token).ContinueWith(_ => { });
    }
    catch (OperationCanceledException)
    {
        break;
    }

    try { await Task.Delay(intervalMs, cts.Token); }
    catch (OperationCanceledException) { break; }
}

producer.Flush(TimeSpan.FromSeconds(5));
Console.WriteLine("Simulador finalizado.");

static double RoundFor(string code, double value)
    => code == "PH" ? Math.Round(value, 2) : Math.Round(value, 1);
