using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Models;

/// <summary>
/// A monitored zone of the orbital greenhouse (e.g. a hydroponic bay, growth module
/// or compartment) where food is produced and where devices are installed.
/// </summary>
public class Region
{
    public int Id { get; set; }

    /// <summary>Short unique code, e.g. "BAY-A1".</summary>
    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = null!;

    /// <summary>Cultivation technology of the module: Hydroponic, Aeroponic, Soil, etc.</summary>
    [MaxLength(60)]
    public string? ModuleType { get; set; }

    /// <summary>Physical placement inside the station, e.g. "Deck 3 / Node 2".</summary>
    [MaxLength(120)]
    public string? FieldLocation { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Device> Devices { get; set; } = new List<Device>();
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
