using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Manage the catalog of environmental metric types.</summary>
[Authorize]
[ApiController]
[Route("api/v1/metric-types")]
[Produces("application/json")]
public class MetricTypesController : ControllerBase
{
    private readonly IMetricTypeService _service;

    public MetricTypesController(IMetricTypeService service) => _service = service;

    /// <summary>Lists all metric types.</summary>
    [HttpGet]
    public async Task<ActionResult<List<MetricTypeResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Gets a metric type by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MetricTypeResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Creates a new metric type.</summary>
    [HttpPost]
    public async Task<ActionResult<MetricTypeResponseDto>> Create([FromBody] MetricTypeCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing metric type.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MetricTypeResponseDto>> Update(int id, [FromBody] MetricTypeUpdateDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deletes a metric type.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
