using Application.DTOs;
using Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
        try
        {
            var policy = await _policyService.CreatePolicyAsync(request);
            return CreatedAtAction(nameof(GetPolicyById), new { id = policy.Id }, policy);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Failed to create policy");
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<PolicyResponse>>> GetAllPolicies(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100;

        var policies = await _policyService.GetAllPoliciesAsync(page, pageSize);
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
}
