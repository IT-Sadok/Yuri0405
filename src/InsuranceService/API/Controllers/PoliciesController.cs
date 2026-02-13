using Application.Commands;
using Application.DTOs;
using Application.Mediator;
using Application.Queries;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoliciesController(IMediator mediator, ILogger<PoliciesController> logger) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<PolicyResponse>> CreatePolicy([FromBody] CreatePolicyRequest request)
    {
        try
        {
            var policy = await mediator.Send(new CreatePolicyCommand(request));
            return CreatedAtAction(nameof(GetPolicyById), new { id = policy.Id }, policy);
        }
        catch (InvalidOperationException ex)
        {
            logger.LogError(ex, "Failed to create policy");
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

        var policies = await mediator.Send(new GetAllPoliciesQuery(page, pageSize));
        return Ok(policies);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PolicyResponse>> GetPolicyById(Guid id)
    {
        var policy = await mediator.Send(new GetPolicyByIdQuery(id));

        if (policy == null)
        {
            return NotFound();
        }

        return Ok(policy);
    }
}
