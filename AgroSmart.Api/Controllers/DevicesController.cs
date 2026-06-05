using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Manage field devices / sensors installed in regions.</summary>
[Authorize]
[ApiController]
[Route("api/v1/devices")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _service;

    public DevicesController(IDeviceService service) => _service = service;

    /// <summary>Lists all devices.</summary>
    [HttpGet]
    public async Task<ActionResult<List<DeviceResponseDto>>> GetAll() => Ok(await _service.GetAllAsync());

    /// <summary>Gets a device by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DeviceResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Lists devices installed in a given region.</summary>
    [HttpGet("by-region/{regionId:int}")]
    public async Task<ActionResult<List<DeviceResponseDto>>> GetByRegion(int regionId)
        => Ok(await _service.GetByRegionAsync(regionId));

    /// <summary>Registers a new device.</summary>
    [HttpPost]
    public async Task<ActionResult<DeviceResponseDto>> Create([FromBody] DeviceCreateDto dto)
    {
        var created = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    /// <summary>Updates an existing device.</summary>
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DeviceResponseDto>> Update(int id, [FromBody] DeviceUpdateDto dto)
        => Ok(await _service.UpdateAsync(id, dto));

    /// <summary>Deletes a device.</summary>
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
