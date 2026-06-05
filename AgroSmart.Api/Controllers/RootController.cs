using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgroSmart.Api.Controllers;

/// <summary>API landing endpoint.</summary>
[ApiController]
[Route("/")]
[ApiExplorerSettings(IgnoreApi = true)]
public class RootController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Index() => Ok(new
    {
        name = "AgroSmart API",
        description = "Monitoramento ambiental e alertas para produção de alimentos no espaço.",
        docs = "/swagger",
        healthcheck = "/api/healthcheck"
    });
}
