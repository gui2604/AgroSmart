using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IMetricTypeService
{
    Task<List<MetricTypeResponseDto>> GetAllAsync();
    Task<MetricTypeResponseDto> GetByIdAsync(int id);
    Task<MetricTypeResponseDto> CreateAsync(MetricTypeCreateDto dto);
    Task<MetricTypeResponseDto> UpdateAsync(int id, MetricTypeUpdateDto dto);
    Task DeleteAsync(int id);
}

public class MetricTypeService : IMetricTypeService
{
    private readonly IMetricTypeRepository _repo;

    public MetricTypeService(IMetricTypeRepository repo) => _repo = repo;

    public async Task<List<MetricTypeResponseDto>> GetAllAsync()
    {
        var items = await _repo.GetAllAsync();
        return items.OrderBy(m => m.Code).Select(m => m.ToDto()).ToList();
    }

    public async Task<MetricTypeResponseDto> GetByIdAsync(int id)
    {
        var metric = await _repo.GetByIdAsync(id) ?? throw NotFoundException.For("Métrica", id);
        return metric.ToDto();
    }

    public async Task<MetricTypeResponseDto> CreateAsync(MetricTypeCreateDto dto)
    {
        var code = dto.Code.Trim().ToUpperInvariant();
        if (await _repo.ExistsAsync(m => m.Code == code))
            throw new ValidationException($"Já existe uma métrica com o código '{code}'.");

        ValidateRange(dto.NominalMin, dto.NominalMax);

        var metric = new MetricType
        {
            Code = code,
            Name = dto.Name,
            Unit = dto.Unit,
            Description = dto.Description,
            NominalMin = dto.NominalMin,
            NominalMax = dto.NominalMax
        };

        await _repo.AddAsync(metric);
        await _repo.SaveChangesAsync();
        return metric.ToDto();
    }

    public async Task<MetricTypeResponseDto> UpdateAsync(int id, MetricTypeUpdateDto dto)
    {
        var metric = await _repo.GetByIdAsync(id) ?? throw NotFoundException.For("Métrica", id);

        ValidateRange(dto.NominalMin, dto.NominalMax);

        metric.Name = dto.Name;
        metric.Unit = dto.Unit;
        metric.Description = dto.Description;
        metric.NominalMin = dto.NominalMin;
        metric.NominalMax = dto.NominalMax;

        _repo.Update(metric);
        await _repo.SaveChangesAsync();
        return metric.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var metric = await _repo.GetByIdAsync(id) ?? throw NotFoundException.For("Métrica", id);
        _repo.Remove(metric);
        await _repo.SaveChangesAsync();
    }

    private static void ValidateRange(double? min, double? max)
    {
        if (min.HasValue && max.HasValue && min > max)
            throw new ValidationException("NominalMin não pode ser maior que NominalMax.");
    }
}
