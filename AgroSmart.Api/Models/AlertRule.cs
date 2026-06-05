using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Models;

/// <summary>
/// A configurable threshold rule for a metric type. When an incoming measurement
/// falls below <see cref="MinThreshold"/> or above <see cref="MaxThreshold"/>,
/// an <see cref="Alert"/> is generated with the configured severity.
/// </summary>
public class AlertRule
{
    public int Id { get; set; }

    [Required]
    [MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }

    public int MetricTypeId { get; set; }
    public MetricType? MetricType { get; set; }

    /// <summary>Optional region scope. When null the rule applies to every region.</summary>
    public int? RegionId { get; set; }
    public Region? Region { get; set; }

    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }

    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}
