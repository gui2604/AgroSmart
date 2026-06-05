using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IAlertRuleService
{
    Task<List<AlertRuleResponseDto>> GetAllAsync();
    Task<AlertRuleResponseDto> GetByIdAsync(int id);
    Task<AlertRuleResponseDto> CreateAsync(AlertRuleCreateDto dto);
    Task<AlertRuleResponseDto> UpdateAsync(int id, AlertRuleUpdateDto dto);
    Task DeleteAsync(int id);
}

public class AlertRuleService : IAlertRuleService
{
    private readonly IAlertRuleRepository _rules;
    private readonly IMetricTypeRepository _metrics;
    private readonly IRegionRepository _regions;

    public AlertRuleService(IAlertRuleRepository rules, IMetricTypeRepository metrics, IRegionRepository regions)
    {
        _rules = rules;
        _metrics = metrics;
        _regions = regions;
    }

    public async Task<List<AlertRuleResponseDto>> GetAllAsync()
    {
        var rules = await _rules.GetAllWithDetailsAsync();
        return rules.Select(r => r.ToDto()).ToList();
    }

    public async Task<AlertRuleResponseDto> GetByIdAsync(int id)
    {
        var rule = await _rules.GetWithDetailsAsync(id) ?? throw NotFoundException.For("Regra de alerta", id);
        return rule.ToDto();
    }

    public async Task<AlertRuleResponseDto> CreateAsync(AlertRuleCreateDto dto)
    {
        await ValidateReferencesAsync(dto.MetricTypeId, dto.RegionId, dto.MinThreshold, dto.MaxThreshold);

        var rule = new AlertRule
        {
            Name = dto.Name,
            Description = dto.Description,
            MetricTypeId = dto.MetricTypeId,
            RegionId = dto.RegionId,
            MinThreshold = dto.MinThreshold,
            MaxThreshold = dto.MaxThreshold,
            Severity = dto.Severity,
            IsActive = dto.IsActive
        };

        await _rules.AddAsync(rule);
        await _rules.SaveChangesAsync();

        var created = await _rules.GetWithDetailsAsync(rule.Id);
        return created!.ToDto();
    }

    public async Task<AlertRuleResponseDto> UpdateAsync(int id, AlertRuleUpdateDto dto)
    {
        var rule = await _rules.GetByIdAsync(id) ?? throw NotFoundException.For("Regra de alerta", id);

        await ValidateReferencesAsync(dto.MetricTypeId, dto.RegionId, dto.MinThreshold, dto.MaxThreshold);

        rule.Name = dto.Name;
        rule.Description = dto.Description;
        rule.MetricTypeId = dto.MetricTypeId;
        rule.RegionId = dto.RegionId;
        rule.MinThreshold = dto.MinThreshold;
        rule.MaxThreshold = dto.MaxThreshold;
        rule.Severity = dto.Severity;
        rule.IsActive = dto.IsActive;

        _rules.Update(rule);
        await _rules.SaveChangesAsync();

        var updated = await _rules.GetWithDetailsAsync(rule.Id);
        return updated!.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var rule = await _rules.GetByIdAsync(id) ?? throw NotFoundException.For("Regra de alerta", id);
        _rules.Remove(rule);
        await _rules.SaveChangesAsync();
    }

    private async Task ValidateReferencesAsync(int metricTypeId, int? regionId, double? min, double? max)
    {
        if (!await _metrics.ExistsAsync(m => m.Id == metricTypeId))
            throw NotFoundException.For("Métrica", metricTypeId);

        if (regionId.HasValue && !await _regions.ExistsAsync(r => r.Id == regionId.Value))
            throw NotFoundException.For("Região", regionId.Value);

        if (min is null && max is null)
            throw new ValidationException("Informe ao menos um limiar (MinThreshold ou MaxThreshold).");

        if (min.HasValue && max.HasValue && min > max)
            throw new ValidationException("MinThreshold não pode ser maior que MaxThreshold.");
    }
}
