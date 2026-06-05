using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Manage greenhouse regions (growth modules / bays).</summary>
[Authorize]
[ApiController]
[Route("api/v1/regions")]
[Produces("application/json")]
public class RegionsController : ControllerBase
{
    private readonly IRegionService _service;

    public RegionsController(IRegionService service) => _service = service;

    /// <summary>Lists all regions.</summary>
    [HttpGet]
    public async Task<ActionResult<List<RegionResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Gets a region by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<RegionResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Creates a new region.</summary>
    [HttpPost]
    public async Task<ActionResult<RegionResponseDto>> Create([FromBody] RegionCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing region.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<RegionResponseDto>> Update(int id, [FromBody] RegionUpdateDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deletes a region (only when it has no linked devices).</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
