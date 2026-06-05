using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Models;

/// <summary>
/// An alert raised when a measurement violated an <see cref="AlertRule"/> threshold,
/// or created manually by an operator. Tracks its own lifecycle (Open / Acknowledged / Resolved).
/// </summary>
public class Alert
{
    public int Id { get; set; }

    [Required]
    [MaxLength(400)]
    public string Message { get; set; } = null!;

    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    public AlertStatus Status { get; set; } = AlertStatus.Open;

    /// <summary>The metric value that triggered the alert.</summary>
    public double TriggeredValue { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }

    /// <summary>Rule that produced the alert. Null for manually created alerts.</summary>
    public int? AlertRuleId { get; set; }
    public AlertRule? AlertRule { get; set; }

    public int MetricTypeId { get; set; }
    public MetricType? MetricType { get; set; }

    /// <summary>Measurement that triggered the alert. Null for manual alerts.</summary>
    public int? MeasurementId { get; set; }
    public Measurement? Measurement { get; set; }

    public int DeviceId { get; set; }
    public Device? Device { get; set; }

    public int RegionId { get; set; }
    public Region? Region { get; set; }

    /// <summary>Operator who acknowledged/resolved the alert, when applicable.</summary>
    public int? AcknowledgedByUserId { get; set; }
    public User? AcknowledgedByUser { get; set; }
}
