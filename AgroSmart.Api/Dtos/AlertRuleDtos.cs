using System.ComponentModel.DataAnnotations;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Dtos;

public class AlertRuleCreateDto
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Required]
    public int MetricTypeId { get; set; }

    /// <summary>Optional. When null the rule applies to every region.</summary>
    public int? RegionId { get; set; }

    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }

    public AlertSeverity Severity { get; set; } = AlertSeverity.Warning;

    public bool IsActive { get; set; } = true;
}

public class AlertRuleUpdateDto
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }

    [Required]
    public int MetricTypeId { get; set; }

    public int? RegionId { get; set; }

    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }

    public AlertSeverity Severity { get; set; }

    public bool IsActive { get; set; }
}

public class AlertRuleResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public int MetricTypeId { get; set; }
    public string? MetricCode { get; set; }
    public int? RegionId { get; set; }
    public string? RegionCode { get; set; }
    public double? MinThreshold { get; set; }
    public double? MaxThreshold { get; set; }
    public string Severity { get; set; } = null!;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
