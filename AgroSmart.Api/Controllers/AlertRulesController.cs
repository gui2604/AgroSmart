using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Configure threshold rules that trigger alerts during ingestion.</summary>
[Authorize]
[ApiController]
[Route("api/v1/alert-rules")]
[Produces("application/json")]
public class AlertRulesController : ControllerBase
{
    private readonly IAlertRuleService _service;

    public AlertRulesController(IAlertRuleService service) => _service = service;

    /// <summary>Lists all alert rules.</summary>
    [HttpGet]
    public async Task<ActionResult<List<AlertRuleResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Gets an alert rule by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<AlertRuleResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Creates a new alert rule.</summary>
    [HttpPost]
    public async Task<ActionResult<AlertRuleResponseDto>> Create([FromBody] AlertRuleCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing alert rule.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<AlertRuleResponseDto>> Update(int id, [FromBody] AlertRuleUpdateDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deletes an alert rule.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
