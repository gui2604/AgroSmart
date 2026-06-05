using System.Globalization;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;
using AgroSmart.Api.Repositories;

namespace AgroSmart.Api.Services;

public interface IIngestionService
{
    /// <summary>Ingests a single sensor reading payload and evaluates alert rules.</summary>
    Task<IngestionResultDto> IngestAsync(SensorReadingIngestDto dto, string? sourceFile = null);

    /// <summary>Ingests a batch of sensor reading payloads (e.g. an uploaded file).</summary>
    Task<BatchIngestionResultDto> IngestBatchAsync(IEnumerable<SensorReadingIngestDto> payloads, string? sourceFile = null);
}

public class IngestionService : IIngestionService
{
    private readonly IDeviceRepository _devices;
    private readonly IMetricTypeRepository _metrics;
    private readonly ISensorReadingRepository _readings;
    private readonly IAlertRuleRepository _rules;
    private readonly IAlertRepository _alerts;
    private readonly ILogger<IngestionService> _logger;

    public IngestionService(
        IDeviceRepository devices,
        IMetricTypeRepository metrics,
        ISensorReadingRepository readings,
        IAlertRuleRepository rules,
        IAlertRepository alerts,
        ILogger<IngestionService> logger)
    {
        _devices = devices;
        _metrics = metrics;
        _readings = readings;
        _rules = rules;
        _alerts = alerts;
        _logger = logger;
    }

    public async Task<IngestionResultDto> IngestAsync(SensorReadingIngestDto dto, string? sourceFile = null)
    {
        var device = await _devices.GetByIdentifierAsync(dto.DeviceIdentifier, tracking: true)
            ?? throw new NotFoundException($"Dispositivo '{dto.DeviceIdentifier}' não está cadastrado.");

        var warnings = new List<string>();
        var metricMap = await _metrics.GetByCodesAsync(dto.Measurements.Select(m => m.MetricCode));

        var reading = new SensorReading
        {
            DeviceId = device.Id,
            CollectedAt = dto.CollectedAt ?? DateTime.UtcNow,
            ReceivedAt = DateTime.UtcNow,
            SourceFile = sourceFile
        };

        foreach (var m in dto.Measurements)
        {
            var code = m.MetricCode.Trim().ToUpperInvariant();
            if (!metricMap.TryGetValue(code, out var metric))
            {
                warnings.Add($"Métrica desconhecida '{m.MetricCode}' ignorada.");
                continue;
            }

            reading.Measurements.Add(new Measurement
            {
                MetricTypeId = metric.Id,
                Value = m.Value
            });
        }

        if (reading.Measurements.Count == 0)
            throw new ValidationException("Nenhuma medição válida foi encontrada no payload.");

        await _readings.AddAsync(reading);
        device.LastSeenAt = reading.ReceivedAt;
        _devices.Update(device);
        await _readings.SaveChangesAsync();

        var triggeredAlerts = await EvaluateRulesAsync(reading, device, metricMap);

        _logger.LogInformation(
            "Leitura {ReadingId} do dispositivo {Device} ingerida: {Measurements} medições, {Alerts} alertas.",
            reading.Id, device.Identifier, reading.Measurements.Count, triggeredAlerts.Count);

        return new IngestionResultDto
        {
            SensorReadingId = reading.Id,
            DeviceIdentifier = device.Identifier,
            CollectedAt = reading.CollectedAt,
            MeasurementsStored = reading.Measurements.Count,
            AlertsTriggered = triggeredAlerts.Count,
            Alerts = triggeredAlerts.Select(a => a.ToDto()).ToList(),
            Warnings = warnings
        };
    }

    public async Task<BatchIngestionResultDto> IngestBatchAsync(IEnumerable<SensorReadingIngestDto> payloads, string? sourceFile = null)
    {
        var batch = new BatchIngestionResultDto();

        foreach (var payload in payloads)
        {
            var result = await IngestAsync(payload, sourceFile);
            batch.Results.Add(result);
            batch.ReadingsProcessed++;
            batch.MeasurementsStored += result.MeasurementsStored;
            batch.AlertsTriggered += result.AlertsTriggered;
        }

        return batch;
    }

    /// <summary>Evaluates each stored measurement against active alert rules and persists alerts.</summary>
    private async Task<List<Alert>> EvaluateRulesAsync(SensorReading reading, Device device, IReadOnlyDictionary<string, MetricType> metricMap)
    {
        var metricById = metricMap.Values.ToDictionary(m => m.Id, m => m);
        var created = new List<Alert>();

        foreach (var measurement in reading.Measurements)
        {
            var rules = await _rules.GetActiveRulesAsync(measurement.MetricTypeId, device.RegionId);

            foreach (var rule in rules)
            {
                var (violated, reason) = AlertThresholdEvaluator.Evaluate(rule, measurement.Value);
                if (!violated) continue;

                metricById.TryGetValue(measurement.MetricTypeId, out var metric);
                var unit = metric?.Unit ?? string.Empty;
                var code = metric?.Code ?? measurement.MetricTypeId.ToString();

                var alert = new Alert
                {
                    Message = $"[{rule.Name}] {code} = {measurement.Value.ToString(CultureInfo.InvariantCulture)} {unit} {reason}.",
                    Severity = rule.Severity,
                    Status = AlertStatus.Open,
                    TriggeredValue = measurement.Value,
                    AlertRuleId = rule.Id,
                    MetricTypeId = measurement.MetricTypeId,
                    MeasurementId = measurement.Id,
                    DeviceId = device.Id,
                    RegionId = device.RegionId,
                    CreatedAt = DateTime.UtcNow
                };

                await _alerts.AddAsync(alert);
                created.Add(alert);
            }
        }

        if (created.Count > 0)
            await _alerts.SaveChangesAsync();

        return created;
    }
}
