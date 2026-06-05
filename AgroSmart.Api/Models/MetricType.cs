using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Models;

/// <summary>
/// Catalog of environmental metrics that devices can measure
/// (humidity, luminosity, oxygen, carbon dioxide, temperature, etc.).
/// </summary>
public class MetricType
{
    public int Id { get; set; }

    /// <summary>Machine code used in the incoming sensor JSON, e.g. "HUMIDITY", "CO2".</summary>
    [Required]
    [MaxLength(30)]
    public string Code { get; set; } = null!;

    [Required]
    [MaxLength(80)]
    public string Name { get; set; } = null!;

    /// <summary>Unit of measurement, e.g. "%", "ppm", "lux", "C".</summary>
    [Required]
    [MaxLength(20)]
    public string Unit { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }

    /// <summary>Lower bound of the ideal/nominal range (informational).</summary>
    public double? NominalMin { get; set; }

    /// <summary>Upper bound of the ideal/nominal range (informational).</summary>
    public double? NominalMax { get; set; }

    public ICollection<Measurement> Measurements { get; set; } = new List<Measurement>();
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
}
