using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Dtos;

/// <summary>
/// Shape of the JSON transmitted by a field device. Mirrors the sample files placed
/// under <c>SampleData/</c>. Each payload represents one collection event and may
/// carry several metric measurements.
/// </summary>
public class SensorReadingIngestDto
{
    /// <summary>Hardware identifier of the device (must already be registered).</summary>
    [Required]
    public string DeviceIdentifier { get; set; } = null!;

    /// <summary>When the sample was collected on the device. Defaults to now when omitted.</summary>
    public DateTime? CollectedAt { get; set; }

    [Required]
    [MinLength(1)]
    public List<MeasurementIngestDto> Measurements { get; set; } = new();
}

public class MeasurementIngestDto
{
    /// <summary>Metric code, matching <c>MetricType.Code</c> (e.g. "CO2").</summary>
    [Required]
    public string MetricCode { get; set; } = null!;

    [Required]
    public double Value { get; set; }
}

/// <summary>Result returned after ingesting one reading.</summary>
public class IngestionResultDto
{
    public int SensorReadingId { get; set; }
    public string DeviceIdentifier { get; set; } = null!;
    public DateTime CollectedAt { get; set; }
    public int MeasurementsStored { get; set; }
    public int AlertsTriggered { get; set; }
    public List<AlertResponseDto> Alerts { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
}

/// <summary>Aggregated result returned after ingesting a batch / uploaded file.</summary>
public class BatchIngestionResultDto
{
    public int ReadingsProcessed { get; set; }
    public int MeasurementsStored { get; set; }
    public int AlertsTriggered { get; set; }
    public List<IngestionResultDto> Results { get; set; } = new();
}
