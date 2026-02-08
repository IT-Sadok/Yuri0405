using Microsoft.AspNetCore.Diagnostics;

namespace API.Helpers;

public class InsuranceExceptionHandler : IExceptionHandler
{
    private readonly ILogger<InsuranceExceptionHandler> _logger;

    public InsuranceExceptionHandler(ILogger<InsuranceExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, errorMessage) = exception switch
        {
            ArgumentException ex => HandleArgumentException(ex),
            InvalidOperationException ex => HandleInvalidOperationException(ex),
            KeyNotFoundException ex => HandleKeyNotFoundException(ex),
            HttpRequestException ex => HandleHttpRequestException(ex),
            _ => HandleGeneralException(exception)
        };

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/json";

        await httpContext.Response.WriteAsJsonAsync(
            new { error = errorMessage },
            cancellationToken);

        return true;
    }

    private (int StatusCode, string Message) HandleArgumentException(ArgumentException ex)
    {
        _logger.LogWarning(ex, "Invalid request argument");
        return (StatusCodes.Status400BadRequest, ex.Message);
    }

    private (int StatusCode, string Message) HandleInvalidOperationException(InvalidOperationException ex)
    {
        _logger.LogWarning(ex, "Invalid operation");
        return (StatusCodes.Status400BadRequest, ex.Message);
    }

    private (int StatusCode, string Message) HandleKeyNotFoundException(KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Resource not found");
        return (StatusCodes.Status404NotFound, ex.Message);
    }

    private (int StatusCode, string Message) HandleHttpRequestException(HttpRequestException ex)
    {
        _logger.LogWarning(ex, "External service unavailable");
        return (StatusCodes.Status504GatewayTimeout, "External service is unavailable");
    }

    private (int StatusCode, string Message) HandleGeneralException(Exception ex)
    {
        _logger.LogError(ex, "Unhandled exception occurred");
        return (StatusCodes.Status500InternalServerError, "Internal server error");
    }
}
