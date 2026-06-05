using System.Text.Json;
using Confluent.Kafka;
using Microsoft.Extensions.Options;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Messaging;

/// <summary>
/// Background service that consumes the continuous stream of sensor reading payloads
/// from Kafka and feeds each one into the ingestion pipeline (persist measurements +
/// evaluate alert rules). This is the consumer side of the real-time data pipeline.
/// </summary>
public class SensorReadingConsumer : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly KafkaSettings _settings;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<SensorReadingConsumer> _logger;

    public SensorReadingConsumer(
        IOptions<KafkaSettings> settings,
        IServiceScopeFactory scopeFactory,
        ILogger<SensorReadingConsumer> logger)
    {
        _settings = settings.Value;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
        // Kafka's Consume call is blocking, so run the poll loop on a dedicated thread
        // to avoid stalling host startup.
        => Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);

    private async Task ConsumeLoop(CancellationToken stoppingToken)
    {
        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };

        using var consumer = new ConsumerBuilder<string, string>(config)
            .SetErrorHandler((_, e) => _logger.LogWarning("Kafka error: {Reason}", e.Reason))
            .Build();

        // The broker may still be starting; retry the subscription until it succeeds.
        await WaitForSubscriptionAsync(consumer, stoppingToken);

        _logger.LogInformation(
            "Kafka consumer iniciado. Tópico '{Topic}', brokers '{Brokers}'.",
            _settings.Topic, _settings.BootstrapServers);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null)
                    continue;

                await ProcessMessageAsync(result.Message.Value, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Falha ao consumir mensagem do Kafka.");
            }
        }

        try { consumer.Close(); } catch { /* ignore shutdown errors */ }
    }

    private async Task WaitForSubscriptionAsync(IConsumer<string, string> consumer, CancellationToken token)
    {
        while (!token.IsCancellationRequested)
        {
            try
            {
                consumer.Subscribe(_settings.Topic);
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Aguardando broker Kafka ({Brokers})... {Message}", _settings.BootstrapServers, ex.Message);
                await Task.Delay(TimeSpan.FromSeconds(5), token);
            }
        }
    }

    private async Task ProcessMessageAsync(string payload, CancellationToken token)
    {
        SensorReadingIngestDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<SensorReadingIngestDto>(payload, JsonOptions);
        }
        catch (JsonException ex)
        {
            _logger.LogWarning(ex, "Mensagem ignorada: JSON inválido.");
            return;
        }

        if (dto is null || string.IsNullOrWhiteSpace(dto.DeviceIdentifier) || dto.Measurements.Count == 0)
        {
            _logger.LogWarning("Mensagem ignorada: payload incompleto.");
            return;
        }

        using var scope = _scopeFactory.CreateScope();
        var ingestion = scope.ServiceProvider.GetRequiredService<IIngestionService>();

        try
        {
            var result = await ingestion.IngestAsync(dto, sourceFile: "kafka-stream");
            _logger.LogInformation(
                "Stream ingerido: dispositivo {Device}, {Measurements} medições, {Alerts} alerta(s).",
                result.DeviceIdentifier, result.MeasurementsStored, result.AlertsTriggered);
        }
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Leitura descartada: {Message}", ex.Message);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning("Leitura inválida: {Message}", ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro inesperado ao ingerir leitura do stream.");
        }
    }
}
