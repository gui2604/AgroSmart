using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Models;

/// <summary>
/// A single collection event transmitted by a device (typically one JSON file),
/// containing one or more individual <see cref="Measurement"/> values.
/// </summary>
public class SensorReading
{
    public int Id { get; set; }

    /// <summary>Timestamp the sample was collected on the device.</summary>
    public DateTime CollectedAt { get; set; }

    /// <summary>Timestamp the reading was received/ingested by the server.</summary>
    public DateTime ReceivedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Name of the originating JSON file, when ingested via file upload.</summary>
    [MaxLength(150)]
    public string? SourceFile { get; set; }

    public int DeviceId { get; set; }
    public Device? Device { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
}
