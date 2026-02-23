using Microsoft.AspNetCore.Diagnostics;

namespace WorldLeague.API.Middleware;

/// <summary>
/// Centralized exception handler. Keeps controllers clean — no try-catch needed.
/// ArgumentException → 400 Bad Request. Unhandled → logged and re-thrown.
/// </summary>
public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        => _logger = logger;

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is ArgumentException argEx)
        {
            httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
            await httpContext.Response.WriteAsJsonAsync(
                new { error = argEx.Message },
                cancellationToken);
            return true;
        }

        _logger.LogError(exception, "Unhandled exception: {Message}", exception.Message);
        return false;
    }
}
