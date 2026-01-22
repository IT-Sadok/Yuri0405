using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoliciesController : ControllerBase
{
    private readonly IPolicyService _policyService;
    private readonly ILogger<PoliciesController> _logger;

    public PoliciesController(IPolicyService policyService, ILogger<PoliciesController> logger)
    {
        _policyService = policyService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult<PolicyResponse>> CreatePolicy([FromBody] CreatePolicyRequest request)
    {
        var customerId = GetCustomerIdFromClaims();
        if (customerId == Guid.Empty)
        {
            return Unauthorized("Customer ID not found in token");
        }

        try
        {
            var policy = await _policyService.CreatePolicyAsync(request, customerId);
            return CreatedAtAction(nameof(GetPolicyById), new { id = policy.Id }, policy);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create policy for customer {CustomerId}", customerId);
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<PolicyResponse>>> GetAllPolicies()
    {
        var policies = await _policyService.GetAllPoliciesAsync();
        return Ok(policies);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PolicyResponse>> GetPolicyById(Guid id)
    {
        var policy = await _policyService.GetPolicyByIdAsync(id);

        if (policy == null)
        {
            return NotFound();
        }

        return Ok(policy);
    }

    [HttpGet("customer/{customerId:guid}")]
    public async Task<ActionResult<IEnumerable<PolicyResponse>>> GetPoliciesByCustomerId(Guid customerId)
    {
        var policies = await _policyService.GetPoliciesByCustomerIdAsync(customerId);
        return Ok(policies);
    }

    private Guid GetCustomerIdFromClaims()
    {
        var customerIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                             ?? User.FindFirst("sub")?.Value;

        if (string.IsNullOrEmpty(customerIdClaim) || !Guid.TryParse(customerIdClaim, out var customerId))
        {
            return Guid.Empty;
        }

        return customerId;
    }
}
