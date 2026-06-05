namespace AgroSmart.Api.Dtos;

public class MeasurementResponseDto
{
    public int Id { get; set; }
    public int MetricTypeId { get; set; }
    public string MetricCode { get; set; } = null!;
    public string MetricUnit { get; set; } = null!;
    public double Value { get; set; }
}

public class SensorReadingResponseDto
{
    public int Id { get; set; }
    public int DeviceId { get; set; }
    public string? DeviceIdentifier { get; set; }
    public DateTime CollectedAt { get; set; }
    public DateTime ReceivedAt { get; set; }
    public string? SourceFile { get; set; }
    public List<MeasurementResponseDto> Measurements { get; set; } = new();
}
