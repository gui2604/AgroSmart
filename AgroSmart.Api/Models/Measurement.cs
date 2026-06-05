namespace AgroSmart.Api.Models;

/// <summary>
/// An individual metric value belonging to a <see cref="SensorReading"/>
/// (e.g. humidity = 65.2). Alerts are evaluated against these values.
/// </summary>
public class Measurement
{
    public int Id { get; set; }

    public double Value { get; set; }

    public int SensorReadingId { get; set; }
    public SensorReading? SensorReading { get; set; }

    public int MetricTypeId { get; set; }
    public MetricType? MetricType { get; set; }

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
