using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace AgroSmart.Api.Exceptions;

/// <summary>
/// Translates domain exceptions into consistent ProblemDetails-style HTTP responses
/// and logs unexpected failures.
/// </summary>
public class GlobalExceptionHandler : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public void OnException(ExceptionContext context)
    {
        var (status, error) = context.Exception switch
        {
            NotFoundException ex => (StatusCodes.Status404NotFound, ex.Message),
            ValidationException ex => (StatusCodes.Status400BadRequest, ex.Message),
            UserNotAuthenticatedException ex => (StatusCodes.Status401Unauthorized, ex.Message),
            DatabaseUnavailableException ex => (StatusCodes.Status503ServiceUnavailable, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor.")
        };

        if (status == StatusCodes.Status500InternalServerError)
            _logger.LogError(context.Exception, "Erro não tratado.");
        else
            _logger.LogWarning("Requisição falhou ({Status}): {Message}", status, context.Exception.Message);

        context.Result = new ObjectResult(new { error, status })
        {
            StatusCode = status
        };
        context.ExceptionHandled = true;
    }
}
