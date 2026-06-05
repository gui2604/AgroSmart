using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Dtos;

public class RegionCreateDto
{
    [Required, MaxLength(30)]
    public string Code { get; set; } = null!;

    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(60)]
    public string? ModuleType { get; set; }

    [MaxLength(120)]
    public string? FieldLocation { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class RegionUpdateDto
{
    [Required, MaxLength(120)]
    public string Name { get; set; } = null!;

    [MaxLength(60)]
    public string? ModuleType { get; set; }

    [MaxLength(120)]
    public string? FieldLocation { get; set; }

    [MaxLength(500)]
    public string? Description { get; set; }
}

public class RegionResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ModuleType { get; set; }
    public string? FieldLocation { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public int DeviceCount { get; set; }
}
