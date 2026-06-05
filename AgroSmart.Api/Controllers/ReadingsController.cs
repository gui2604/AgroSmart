using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Query stored sensor readings and their measurements.</summary>
[Authorize]
[ApiController]
[Route("api/v1/readings")]
[Produces("application/json")]
public class ReadingsController : ControllerBase
{
    private readonly IReadingService _service;

    public ReadingsController(IReadingService service) => _service = service;

    /// <summary>Gets a reading (with measurements) by id.</summary>
    [HttpGet("{id:int}")]
    public async Task<ActionResult<SensorReadingResponseDto>> GetById(int id) => Ok(await _service.GetByIdAsync(id));

    /// <summary>Lists the most recent readings for a device.</summary>
    [HttpGet("by-device/{deviceId:int}")]
    public async Task<ActionResult<List<SensorReadingResponseDto>>> GetByDevice(int deviceId, [FromQuery] int take = 50)
        => Ok(await _service.GetByDeviceAsync(deviceId, take));
}
