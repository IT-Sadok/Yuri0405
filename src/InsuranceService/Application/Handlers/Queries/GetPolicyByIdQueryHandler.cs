using Application.DTOs;
using Application.Interfaces;
using Application.Mediator;
using Application.Queries;

namespace Application.Handlers.Queries;

public class GetPolicyByIdQueryHandler(IPolicyRepository policyRepository)
    : IRequestHandler<GetPolicyByIdQuery, PolicyResponse?>
{
    public async Task<PolicyResponse?> Handle(GetPolicyByIdQuery query, CancellationToken cancellationToken = default)
    {
        var policy = await policyRepository.GetByIdAsync(query.Id, cancellationToken);
        if (policy == null) return null;

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
