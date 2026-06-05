namespace AgroSmart.Api.Messaging;

/// <summary>
/// Configuration for the Kafka streaming pipeline. Bound from the "Kafka" section
/// of configuration (appsettings / environment variables).
/// </summary>
public class KafkaSettings
{
    public const string SectionName = "Kafka";

    /// <summary>When false the consumer hosted service is not registered (e.g. plain local run without a broker).</summary>
    public bool Enabled { get; set; }

    /// <summary>Broker list, e.g. "localhost:9092" or "kafka:9092" inside Docker.</summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Topic carrying simulated sensor reading payloads.</summary>
    public string Topic { get; set; } = "agrosmart.sensor-readings";

    /// <summary>Consumer group id used by the API ingestion consumer.</summary>
    public string GroupId { get; set; } = "agrosmart-api";
}
