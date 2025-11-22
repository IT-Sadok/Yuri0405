using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PaymentService.Models.DTOs;
using PaymentService.Models.Enums;
using PaymentService.Services;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        // Extract user ID from JWT claims
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            return Unauthorized(new { error = "Invalid or missing user ID in token" });
        }

        // Set the user ID from the JWT token
        request.UserId = userId;

        var response = await _paymentService.ProcessPaymentAsync(request, idempotencyKey);
        return Ok(response);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var response = await _paymentService.GetPaymentAsync(id);
        return Ok(response);
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPayments()
    {
        var response = await _paymentService.GetAllPaymentsAsync();
        return Ok(response);
    }
}