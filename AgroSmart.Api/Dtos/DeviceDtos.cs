using System.ComponentModel.DataAnnotations;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Dtos;

public class DeviceCreateDto
{
    [Required, MaxLength(50)]
    public string Identifier { get; set; } = null!;

    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(60)]
    public string? DeviceType { get; set; }

    [MaxLength(30)]
    public string? FirmwareVersion { get; set; }

    public DeviceStatus Status { get; set; } = DeviceStatus.Active;

    [Required]
    public int RegionId { get; set; }
}

public class DeviceUpdateDto
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(60)]
    public string? DeviceType { get; set; }

    [MaxLength(30)]
    public string? FirmwareVersion { get; set; }

    public DeviceStatus Status { get; set; }

    [Required]
    public int RegionId { get; set; }
}

public class DeviceResponseDto
{
    public int Id { get; set; }
    public string Identifier { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? DeviceType { get; set; }
    public string Status { get; set; } = null!;
    public string? FirmwareVersion { get; set; }
    public DateTime InstalledAt { get; set; }
    public DateTime? LastSeenAt { get; set; }
    public int RegionId { get; set; }
    public string? RegionCode { get; set; }
}
