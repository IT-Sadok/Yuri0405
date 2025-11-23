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
    private readonly ICurrentUserService _currentUserService;

    public PaymentController(ILogger<PaymentController> logger,
        IPaymentService paymentService,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _paymentService = paymentService;
        _currentUserService = currentUserService;
    }

    [HttpPost]
    [RequiredHeader("Idempotency-Key")]
    public async Task<IActionResult> CreatePayment(
        [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
        [FromBody] PaymentRequest request)
    {
        var userId = _currentUserService.GetUserId();
        if (userId == null)
        {
            return Unauthorized(new { error = "Invalid or missing user ID in token" });
        }
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