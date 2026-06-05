using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Analytical reports on region salubrity and platform-wide alerts.</summary>
[Authorize]
[ApiController]
[Route("api/v1/reports")]
[Produces("application/json")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _service;

    public ReportsController(IReportService service) => _service = service;

    /// <summary>Salubrity (health) report for every region, with latest metric snapshots.</summary>
    [HttpGet("region-health")]
    public async Task<ActionResult<List<RegionHealthReportDto>>> RegionHealth()
        => Ok(await _service.GetRegionHealthAsync());

    /// <summary>Salubrity report for a single region.</summary>
    [HttpGet("region-health/{regionId:int}")]
    public async Task<ActionResult<RegionHealthReportDto>> RegionHealthById(int regionId)
        => Ok(await _service.GetRegionHealthAsync(regionId));

    /// <summary>Aggregated alert statistics across the platform.</summary>
    [HttpGet("alerts-summary")]
    public async Task<ActionResult<AlertSummaryReportDto>> AlertSummary()
        => Ok(await _service.GetAlertSummaryAsync());
}
