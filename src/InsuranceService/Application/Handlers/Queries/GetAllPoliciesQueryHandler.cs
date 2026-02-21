using Application.DTOs;
using Application.Interfaces;
using Application.Mediator;
using Application.Queries;

namespace Application.Handlers.Queries;

public class GetAllPoliciesQueryHandler(IPolicyRepository policyRepository)
    : IRequestHandler<GetAllPoliciesQuery, PagedResponse<PolicyResponse>>
{
    public async Task<PagedResponse<PolicyResponse>> Handle(GetAllPoliciesQuery query, CancellationToken cancellationToken = default)
    {
        var (policies, totalCount) = await policyRepository.GetAllAsync(query.Page, query.PageSize, cancellationToken);

        return new PagedResponse<PolicyResponse>
        {
            Items = policies.Select(p => new PolicyResponse
            {
                Id = p.Id,
                Name = p.Name,
                Description = p.Description,
                ProductType = p.ProductType,
                CoverageAmount = p.CoverageAmount,
                PremiumAmount = p.PremiumAmount,
                DurationMonths = p.DurationMonths,
                Status = p.Status,
                CreatedAt = p.CreatedAt
            }),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }
}
