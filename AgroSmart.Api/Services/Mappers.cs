using AgroSmart.Api.Dtos;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Services;

/// <summary>Centralised entity-to-DTO projections.</summary>
public static class Mappers
{
    public static RegionResponseDto ToDto(this Region r) => new()
    {
        Id = r.Id,
        Code = r.Code,
        Name = r.Name,
        ModuleType = r.ModuleType,
        FieldLocation = r.FieldLocation,
        Description = r.Description,
        CreatedAt = r.CreatedAt,
        DeviceCount = r.Devices?.Count ?? 0
    };

    public static DeviceResponseDto ToDto(this Device d) => new()
    {
        Id = d.Id,
        Identifier = d.Identifier,
        Name = d.Name,
        DeviceType = d.DeviceType,
        Status = d.Status.ToString(),
        FirmwareVersion = d.FirmwareVersion,
        InstalledAt = d.InstalledAt,
        LastSeenAt = d.LastSeenAt,
        RegionId = d.RegionId,
        RegionCode = d.Region?.Code
    };

    public static MetricTypeResponseDto ToDto(this MetricType m) => new()
    {
        Id = m.Id,
        Code = m.Code,
        Name = m.Name,
        Unit = m.Unit,
        Description = m.Description,
        NominalMin = m.NominalMin,
        NominalMax = m.NominalMax
    };

    public static AlertRuleResponseDto ToDto(this AlertRule r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        MetricTypeId = r.MetricTypeId,
        MetricCode = r.MetricType?.Code,
        RegionId = r.RegionId,
        RegionCode = r.Region?.Code,
        MinThreshold = r.MinThreshold,
        MaxThreshold = r.MaxThreshold,
        Severity = r.Severity.ToString(),
        IsActive = r.IsActive,
        CreatedAt = r.CreatedAt
    };

    public static AlertResponseDto ToDto(this Alert a) => new()
    {
        Id = a.Id,
        Message = a.Message,
        Severity = a.Severity.ToString(),
        Status = a.Status.ToString(),
        TriggeredValue = a.TriggeredValue,
        CreatedAt = a.CreatedAt,
        AcknowledgedAt = a.AcknowledgedAt,
        ResolvedAt = a.ResolvedAt,
        AlertRuleId = a.AlertRuleId,
        AlertRuleName = a.AlertRule?.Name,
        MetricTypeId = a.MetricTypeId,
        MetricCode = a.MetricType?.Code,
        MetricUnit = a.MetricType?.Unit,
        MeasurementId = a.MeasurementId,
        DeviceId = a.DeviceId,
        DeviceIdentifier = a.Device?.Identifier,
        RegionId = a.RegionId,
        RegionCode = a.Region?.Code,
        AcknowledgedByUserId = a.AcknowledgedByUserId
    };

    public static MeasurementResponseDto ToDto(this Measurement m) => new()
    {
        Id = m.Id,
        MetricTypeId = m.MetricTypeId,
        MetricCode = m.MetricType?.Code ?? string.Empty,
        MetricUnit = m.MetricType?.Unit ?? string.Empty,
        Value = m.Value
    };

    public static SensorReadingResponseDto ToDto(this SensorReading r) => new()
    {
        Id = r.Id,
        DeviceId = r.DeviceId,
        DeviceIdentifier = r.Device?.Identifier,
        CollectedAt = r.CollectedAt,
        ReceivedAt = r.ReceivedAt,
        SourceFile = r.SourceFile,
        Measurements = r.Measurements?.Select(m => m.ToDto()).ToList() ?? new()
    };
}
