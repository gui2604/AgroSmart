using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Models;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Query and manage alerts (manual creation, lifecycle, deletion).</summary>
[Authorize]
[ApiController]
[Route("api/v1/alerts")]
[Produces("application/json")]
public class AlertsController : ControllerBase
{
    private readonly IAlertService _service;

    public AlertsController(IAlertService service) => _service = service;

    /// <summary>Lists alerts, optionally filtered by region, device, status and severity.</summary>
    [HttpGet]
    public async Task<ActionResult<List<AlertResponseDto>>> GetAll(
        [FromQuery] int? regionId,
        [FromQuery] int? deviceId,
        [FromQuery] AlertStatus? status,
        [FromQuery] AlertSeverity? severity)
        => Ok(await _service.GetAllAsync(regionId, deviceId, status, severity));

    /// <summary>Gets an alert by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlertResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Creates an alert manually (outside the ingestion engine).</summary>
    [HttpPost]
    public async Task<ActionResult<AlertResponseDto>> Create([FromBody] AlertCreateDto dto)
    {
        var created = await _service.CreateManualAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an alert's lifecycle status (Open / Acknowledged / Resolved).</summary>
    [HttpPut("{id:int}/status")]
    public async Task<ActionResult<AlertResponseDto>> UpdateStatus(int id, [FromBody] AlertStatusUpdateDto dto)
        => Ok(await _service.UpdateStatusAsync(id, dto.Status, GetUserId()));

    /// <summary>Deletes an alert.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }

    private int? GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : null;
    }
}
