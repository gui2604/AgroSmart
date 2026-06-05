using System.ComponentModel.DataAnnotations;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Dtos;

/// <summary>Payload for manually raising an alert (not produced by the ingestion engine).</summary>
public class AlertCreateDto
{
    [Required, MaxLength(400)]
    public string Message { get; set; } = null!;

    [Required]
    public int DeviceId { get; set; }

    [Required]
    public int MetricTypeId { get; set; }

    public double TriggeredValue { get; set; }

    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;
}

/// <summary>Payload for updating an alert's lifecycle status.</summary>
public class AlertStatusUpdateDto
{
    [Required]
    public AlertStatus Status { get; set; }
}

public class AlertResponseDto
{
    public int Id { get; set; }
    public string Message { get; set; } = null!;
    public string Severity { get; set; } = null!;
    public string Status { get; set; } = null!;
    public double TriggeredValue { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? AcknowledgedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int? AlertRuleId { get; set; }
    public string? AlertRuleName { get; set; }
    public int MetricTypeId { get; set; }
    public string? MetricCode { get; set; }
    public string? MetricUnit { get; set; }
    public int? MeasurementId { get; set; }
    public int DeviceId { get; set; }
    public string? DeviceIdentifier { get; set; }
    public int RegionId { get; set; }
    public string? RegionCode { get; set; }
    public int? AcknowledgedByUserId { get; set; }
}
