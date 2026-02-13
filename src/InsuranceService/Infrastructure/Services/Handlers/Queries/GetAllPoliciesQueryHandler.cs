using Application.DTOs;
using Application.Mediator;
using Application.Queries;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Handlers.Queries;

public class GetAllPoliciesQueryHandler(InsuranceDbContext context)
    : IRequestHandler<GetAllPoliciesQuery, PagedResponse<PolicyResponse>>
{
    public async Task<PagedResponse<PolicyResponse>> Handle(GetAllPoliciesQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = context.Policies.OrderByDescending(p => p.CreatedAt);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var policies = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<PolicyResponse>
        {
            Items = policies.Select(MapToResponse),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
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
