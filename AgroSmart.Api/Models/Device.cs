using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Models;

/// <summary>
/// A field device / sensor installed in a region. Devices collect environmental
/// metrics and periodically transmit them as JSON readings.
/// </summary>
public class Device
{
    public int Id { get; set; }

    /// <summary>Unique hardware identifier reported in the sensor JSON, e.g. "SENSOR-A1-01".</summary>
    [Required]
    [MaxLength(50)]
    public string Identifier { get; set; } = null!;

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = null!;

    /// <summary>Kind of device, e.g. "MultiSensor", "CO2 Probe", "PAR Light Sensor".</summary>
    [MaxLength(60)]
    public string? DeviceType { get; set; }

    public DeviceStatus Status { get; set; } = DeviceStatus.Active;

    [MaxLength(30)]
    public string? FirmwareVersion { get; set; }

    public DateTime InstalledAt { get; set; } = DateTime.UtcNow;

    /// <summary>Timestamp of the last received reading (updated on ingestion).</summary>
    public DateTime? LastSeenAt { get; set; }

    public int RegionId { get; set; }
    public Region? Region { get; set; }

    public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
