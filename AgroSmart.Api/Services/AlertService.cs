using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IAlertService
{
    Task<List<AlertResponseDto>> GetAllAsync(int? regionId, int? deviceId, AlertStatus? status, AlertSeverity? severity);
    Task<AlertResponseDto> GetByIdAsync(int id);
    Task<AlertResponseDto> CreateManualAsync(AlertCreateDto dto);
    Task<AlertResponseDto> UpdateStatusAsync(int id, AlertStatus status, int? userId);
    Task DeleteAsync(int id);
}

public class AlertService : IAlertService
{
    private readonly IAlertRepository _alerts;
    private readonly IDeviceRepository _devices;
    private readonly IMetricTypeRepository _metrics;

    public AlertService(IAlertRepository alerts, IDeviceRepository devices, IMetricTypeRepository metrics)
    {
        _alerts = alerts;
        _devices = devices;
        _metrics = metrics;
    }

    public async Task<List<AlertResponseDto>> GetAllAsync(int? regionId, int? deviceId, AlertStatus? status, AlertSeverity? severity)
    {
        var alerts = await _alerts.GetFilteredAsync(regionId, deviceId, status, severity);
        return alerts.Select(a => a.ToDto()).ToList();
    }

    public async Task<AlertResponseDto> GetByIdAsync(int id)
    {
        var alert = await _alerts.GetWithDetailsAsync(id) ?? throw NotFoundException.For("Alerta", id);
        return alert.ToDto();
    }

    public async Task<AlertResponseDto> CreateManualAsync(AlertCreateDto dto)
    {
        var device = await _devices.GetByIdAsync(dto.DeviceId) ?? throw NotFoundException.For("Dispositivo", dto.DeviceId);

        if (!await _metrics.ExistsAsync(m => m.Id == dto.MetricTypeId))
            throw NotFoundException.For("Métrica", dto.MetricTypeId);

        var alert = new Alert
        {
            Message = dto.Message,
            Severity = dto.Severity,
            Status = AlertStatus.Open,
            TriggeredValue = dto.TriggeredValue,
            MetricTypeId = dto.MetricTypeId,
            DeviceId = device.Id,
            RegionId = device.RegionId,
            CreatedAt = DateTime.UtcNow
        };

        await _alerts.AddAsync(alert);
        await _alerts.SaveChangesAsync();

        var created = await _alerts.GetWithDetailsAsync(alert.Id);
        return created!.ToDto();
    }

    public async Task<AlertResponseDto> UpdateStatusAsync(int id, AlertStatus status, int? userId)
    {
        var alert = await _alerts.GetWithDetailsAsync(id) ?? throw NotFoundException.For("Alerta", id);

        alert.Status = status;
        switch (status)
        {
            case AlertStatus.Acknowledged:
                alert.AcknowledgedAt = DateTime.UtcNow;
                alert.AcknowledgedByUserId = userId;
                break;
            case AlertStatus.Resolved:
                alert.ResolvedAt = DateTime.UtcNow;
                alert.AcknowledgedByUserId ??= userId;
                alert.AcknowledgedAt ??= DateTime.UtcNow;
                break;
            case AlertStatus.Open:
                alert.AcknowledgedAt = null;
                alert.ResolvedAt = null;
                alert.AcknowledgedByUserId = null;
                break;
        }

        _alerts.Update(alert);
        await _alerts.SaveChangesAsync();
        return alert.ToDto();
    }

    public async Task DeleteAsync(int id)
    {
        var alert = await _alerts.GetByIdAsync(id) ?? throw NotFoundException.For("Alerta", id);
        _alerts.Remove(alert);
        await _alerts.SaveChangesAsync();
    }
}
