using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AgroSmart.Api.Dtos;
using AgroSmart.Api.Services;

namespace AgroSmart.Api.Controllers;

/// <summary>Authentication endpoints (registration and JWT login).</summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IUserService _service;

    public AuthController(IUserService service) => _service = service;

    /// <summary>Registers a new operator and returns a JWT token.</summary>
    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register([FromBody] UserRegisterDto dto)
        => Ok(await _service.RegisterAsync(dto));

    /// <summary>Authenticates an operator and returns a JWT token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login([FromBody] UserLoginDto dto)
        => Ok(await _service.LoginAsync(dto));
}
