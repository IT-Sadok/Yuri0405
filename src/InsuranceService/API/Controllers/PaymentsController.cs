using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
    {
        _paymentService = paymentService;
        _logger = logger;
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<PaymentInitiationResponse>> InitiatePayment([FromBody] InitiatePaymentRequest request)
    {
        var jwtToken = Request.Headers["Authorization"].ToString();
        if (string.IsNullOrEmpty(jwtToken))
        {
            return Unauthorized("Authorization token is required");
        }

        try
        {
            var response = await _paymentService.InitiatePaymentAsync(request, jwtToken);
            return Ok(response);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to initiate payment for policy {PolicyId}", request.PolicyId);
            return BadRequest(ex.Message);
        }
    }
}
