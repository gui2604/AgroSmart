using Microsoft.EntityFrameworkCore;
using AgroSmart.Api.Data;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Exceptions;
using AgroSmart.Api.Models;

namespace AgroSmart.Api.Services;

public interface IReportService
{
    Task<List<RegionHealthReportDto>> GetRegionHealthAsync();
    Task<RegionHealthReportDto> GetRegionHealthAsync(int regionId);
    Task<AlertSummaryReportDto> GetAlertSummaryAsync();
}

/// <summary>
/// Read-model reporting service. Aggregates measurements and alerts to expose the
/// salubrity (health) of each region and a platform-wide alert summary.
/// </summary>
public class ReportService : IReportService
{
    private readonly ApplicationDbContext _ctx;

    public ReportService(ApplicationDbContext ctx) => _ctx = ctx;

    public async Task<List<RegionHealthReportDto>> GetRegionHealthAsync()
    {
        var regionIds = await _ctx.Regions.Select(r => r.Id).ToListAsync();
        var reports = new List<RegionHealthReportDto>();
        foreach (var id in regionIds)
            reports.Add(await BuildRegionReportAsync(id));
        return reports;
    }

    public Task<RegionHealthReportDto> GetRegionHealthAsync(int regionId) => BuildRegionReportAsync(regionId);

    private async Task<RegionHealthReportDto> BuildRegionReportAsync(int regionId)
    {
        var region = await _ctx.Regions.AsNoTracking().FirstOrDefaultAsync(r => r.Id == regionId)
            ?? throw NotFoundException.For("Região", regionId);

        var deviceIds = await _ctx.Devices.Where(d => d.RegionId == regionId).Select(d => d.Id).ToListAsync();

        var totalAlerts = await _ctx.Alerts.CountAsync(a => a.RegionId == regionId);
        var openAlerts = await _ctx.Alerts.CountAsync(a => a.RegionId == regionId && a.Status == AlertStatus.Open);
        var criticalAlerts = await _ctx.Alerts.CountAsync(a =>
            a.RegionId == regionId && a.Severity == AlertSeverity.Critical && a.Status == AlertStatus.Open);

        DateTime? lastReadingAt = deviceIds.Count == 0
            ? null
            : await _ctx.SensorReadings
                .Where(r => deviceIds.Contains(r.DeviceId))
                .OrderByDescending(r => r.CollectedAt)
                .Select(r => (DateTime?)r.CollectedAt)
                .FirstOrDefaultAsync();

        var latestMetrics = await BuildLatestMetricsAsync(deviceIds);

        var status = criticalAlerts > 0
            ? "Critical"
            : (openAlerts > 0 || latestMetrics.Any(m => !m.WithinNominal) ? "Attention" : "Healthy");

        return new RegionHealthReportDto
        {
            RegionId = region.Id,
            RegionCode = region.Code,
            RegionName = region.Name,
            DeviceCount = deviceIds.Count,
            TotalAlerts = totalAlerts,
            OpenAlerts = openAlerts,
            CriticalAlerts = criticalAlerts,
            LastReadingAt = lastReadingAt,
            SalubrityStatus = status,
            LatestMetrics = latestMetrics
        };
    }

    private async Task<List<MetricSnapshotDto>> BuildLatestMetricsAsync(List<int> deviceIds)
    {
        if (deviceIds.Count == 0) return new();

        // Pull the recent measurements for the region's devices and reduce in memory
        // to the latest value per metric (simulation-scale data volumes).
        var rows = await _ctx.Measurements
            .Where(m => deviceIds.Contains(m.SensorReading!.DeviceId))
            .OrderByDescending(m => m.SensorReading!.CollectedAt)
            .Select(m => new
            {
                m.MetricTypeId,
                m.MetricType!.Code,
                m.MetricType.Unit,
                m.MetricType.NominalMin,
                m.MetricType.NominalMax,
                m.Value,
                CollectedAt = m.SensorReading!.CollectedAt
            })
            .Take(1000)
            .ToListAsync();

        return rows
            .GroupBy(r => r.MetricTypeId)
            .Select(g =>
            {
                var latest = g.OrderByDescending(x => x.CollectedAt).First();
                var withinNominal =
                    (latest.NominalMin is null || latest.Value >= latest.NominalMin) &&
                    (latest.NominalMax is null || latest.Value <= latest.NominalMax);

                return new MetricSnapshotDto
                {
                    MetricCode = latest.Code,
                    Unit = latest.Unit,
                    LatestValue = latest.Value,
                    NominalMin = latest.NominalMin,
                    NominalMax = latest.NominalMax,
                    WithinNominal = withinNominal,
                    MeasuredAt = latest.CollectedAt
                };
            })
            .OrderBy(m => m.MetricCode)
            .ToList();
    }

    public async Task<AlertSummaryReportDto> GetAlertSummaryAsync()
    {
        var summary = new AlertSummaryReportDto
        {
            TotalAlerts = await _ctx.Alerts.CountAsync(),
            OpenAlerts = await _ctx.Alerts.CountAsync(a => a.Status == AlertStatus.Open),
            AcknowledgedAlerts = await _ctx.Alerts.CountAsync(a => a.Status == AlertStatus.Acknowledged),
            ResolvedAlerts = await _ctx.Alerts.CountAsync(a => a.Status == AlertStatus.Resolved),
            CriticalAlerts = await _ctx.Alerts.CountAsync(a => a.Severity == AlertSeverity.Critical),
            WarningAlerts = await _ctx.Alerts.CountAsync(a => a.Severity == AlertSeverity.Warning),
            InfoAlerts = await _ctx.Alerts.CountAsync(a => a.Severity == AlertSeverity.Info)
        };

        summary.ByMetric = await _ctx.Alerts
            .GroupBy(a => a.MetricType!.Code)
            .Select(g => new AlertCountByMetricDto { MetricCode = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        summary.ByRegion = await _ctx.Alerts
            .GroupBy(a => a.Region!.Code)
            .Select(g => new AlertCountByRegionDto { RegionCode = g.Key, Count = g.Count() })
            .OrderByDescending(x => x.Count)
            .ToListAsync();

        return summary;
    }
}
