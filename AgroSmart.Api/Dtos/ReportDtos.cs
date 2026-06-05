namespace AgroSmart.Api.Dtos;

/// <summary>High level salubrity summary for a single region.</summary>
public class RegionHealthReportDto
{
    public int RegionId { get; set; }
    public string RegionCode { get; set; } = null!;
    public string RegionName { get; set; } = null!;
    public int DeviceCount { get; set; }
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public DateTime? LastReadingAt { get; set; }
    /// <summary>Computed status: Healthy / Attention / Critical.</summary>
    public string SalubrityStatus { get; set; } = null!;
    public List<MetricSnapshotDto> LatestMetrics { get; set; } = new();
}

/// <summary>Latest measured value for a metric in a region.</summary>
public class MetricSnapshotDto
{
    public string MetricCode { get; set; } = null!;
    public string Unit { get; set; } = null!;
    public double? LatestValue { get; set; }
    public double? NominalMin { get; set; }
    public double? NominalMax { get; set; }
    public bool WithinNominal { get; set; }
    public DateTime? MeasuredAt { get; set; }
}

/// <summary>Aggregated alert statistics across the whole platform.</summary>
public class AlertSummaryReportDto
{
    public int TotalAlerts { get; set; }
    public int OpenAlerts { get; set; }
    public int AcknowledgedAlerts { get; set; }
    public int ResolvedAlerts { get; set; }
    public int CriticalAlerts { get; set; }
    public int WarningAlerts { get; set; }
    public int InfoAlerts { get; set; }
    public List<AlertCountByMetricDto> ByMetric { get; set; } = new();
    public List<AlertCountByRegionDto> ByRegion { get; set; } = new();
}

public class AlertCountByMetricDto
{
    public string MetricCode { get; set; } = null!;
    public int Count { get; set; }
}

public class AlertCountByRegionDto
{
    public string RegionCode { get; set; } = null!;
    public int Count { get; set; }
}
