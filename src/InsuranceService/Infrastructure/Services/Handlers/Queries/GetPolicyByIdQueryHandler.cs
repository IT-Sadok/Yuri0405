using Application.DTOs;
using Application.Mediator;
using Application.Queries;
using Domain.Entities;
using Infrastructure.Data;

namespace Infrastructure.Services.Handlers.Queries;

public class GetPolicyByIdQueryHandler(InsuranceDbContext context)
    : IRequestHandler<GetPolicyByIdQuery, PolicyResponse?>
{
    public async Task<PolicyResponse?> Handle(GetPolicyByIdQuery query, CancellationToken cancellationToken = default)
    {
        var policy = await context.Policies.FindAsync([query.Id], cancellationToken);
        return policy != null ? MapToResponse(policy) : null;
    }

    private static PolicyResponse MapToResponse(Policy policy)
    {
        return new PolicyResponse
        {
            Id = policy.Id,
            Name = policy.Name,
            Description = policy.Description,
            ProductType = policy.ProductType,
            CoverageAmount = policy.CoverageAmount,
            PremiumAmount = policy.PremiumAmount,
            DurationMonths = policy.DurationMonths,
            Status = policy.Status,
            CreatedAt = policy.CreatedAt
        };
    }
}
