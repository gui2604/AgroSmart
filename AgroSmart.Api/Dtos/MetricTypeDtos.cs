using System.ComponentModel.DataAnnotations;

namespace AgroSmart.Api.Dtos;

public class MetricTypeCreateDto
{
    [Required, MaxLength(30)]
    public string Code { get; set; } = null!;

    [Required, MaxLength(80)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Unit { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }

    public double? NominalMin { get; set; }
    public double? NominalMax { get; set; }
}

public class MetricTypeUpdateDto
{
    [Required, MaxLength(80)]
    public string Name { get; set; } = null!;

    [Required, MaxLength(20)]
    public string Unit { get; set; } = null!;

    [MaxLength(300)]
    public string? Description { get; set; }

    public double? NominalMin { get; set; }
    public double? NominalMax { get; set; }
}

public class MetricTypeResponseDto
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public string? Description { get; set; }
    public double? NominalMin { get; set; }
    public double? NominalMax { get; set; }
}
