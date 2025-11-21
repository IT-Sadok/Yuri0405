using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace PaymentService.Helpers;

public class PaymentExceptionHandler : IExceptionHandler
{
    private readonly ILogger<PaymentExceptionHandler> _logger;

    public PaymentExceptionHandler(ILogger<PaymentExceptionHandler> logger)
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
            HttpRequestException ex => HandleHttpRequestException(ex),
            KeyNotFoundException ex => HandleKeyNotFoundException(ex),
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
        _logger.LogWarning(ex, "Invalid payment request");
        return (StatusCodes.Status400BadRequest, ex.Message);
    }

    private (int StatusCode, string Message) HandleHttpRequestException(HttpRequestException ex)
    {
        _logger.LogWarning(ex, "Payment gateway is unavailable");
        return (StatusCodes.Status504GatewayTimeout, "Payment gateway is unavailable");
    }

    private (int StatusCode, string Message) HandleKeyNotFoundException(KeyNotFoundException ex)
    {
        _logger.LogWarning(ex, "Payment not found");
        return (StatusCodes.Status404NotFound, ex.Message);
    }

    private (int StatusCode, string Message) HandleGeneralException(Exception ex)
    {
        _logger.LogError(ex, "Failed to process payment operation");
        return (StatusCodes.Status500InternalServerError, "Internal server error");
    }
}
