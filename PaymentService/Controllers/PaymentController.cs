using Microsoft.AspNetCore.Mvc;
using PaymentService.Models.DTOs;
using PaymentService.Models.Enums;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController:ControllerBase
{
    private readonly ILogger<PaymentController> _logger;
    private readonly IPaymentService _paymentService;

    public PaymentController(ILogger<PaymentController> logger,
        IPaymentService paymentService)
    {
        _logger = logger;
        _paymentService = paymentService;
    }

    [HttpPost]
    [RequiredHeader("Idempotency-Key")]
    public async Task<IActionResult> CreatePayment(
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        [FromBody] PaymentRequest request)
    {
        try
        {
            var response = await _paymentService.ProcessPaymentAsync(request, idempotencyKey);
            if (response.Status == "failed")
                return StatusCode(402, new { error = response.Message });
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid payment request");
            return BadRequest(new { error = ex.Message });
        }
        catch (HttpRequestException ex)
        {
            return StatusCode(504, new { error = "Payment gateway is unavailable"});
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process payment");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        try
        {
            var response = await _paymentService.GetPaymentAsync(id);
            return Ok(response);
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning("Payment {PaymentId} not found", id);
            return NotFound(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve payment {PaymentId}", id);
            return StatusCode(500, new { error = "Internal server error" });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPayments()
    {
        try
        {
            var response = await _paymentService.GetAllPaymentsAsync();
            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve payments");
            return StatusCode(500, new { error = "Internal server error" });
        }
    }
}