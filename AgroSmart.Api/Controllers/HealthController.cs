using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Data;
using AgroSmart.Api.Exceptions;

namespace AgroSmart.Api.Controllers;

/// <summary>Liveness and readiness checks.</summary>
[ApiController]
[Route("api/healthcheck")]
[Produces("application/json")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public HealthController(ApplicationDbContext context) => _context = context;

    /// <summary>Basic liveness probe (no dependencies).</summary>
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Basic() => Ok(new { status = "Healthy", timestamp = DateTime.UtcNow });

    /// <summary>Full readiness probe including Oracle connectivity.</summary>
    [HttpGet("full")]
    [Authorize]
    public async Task<IActionResult> Full()
    {
        bool canConnect;
        try
        {
            canConnect = await _context.Database.CanConnectAsync();
        }
        catch (Exception ex)
        {
            throw new DatabaseUnavailableException($"Falha ao conectar ao Oracle: {ex.Message}");
        }

        if (!canConnect)
            throw new DatabaseUnavailableException("Não foi possível conectar ao banco de dados Oracle.");

        return Ok(new { status = "Healthy", database = "Connected", provider = "Oracle", timestamp = DateTime.UtcNow });
    }
}
