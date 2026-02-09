using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Queries;

public class PolicyQueryService : IPolicyQueryService
{
    private readonly InsuranceDbContext _context;

    public PolicyQueryService(InsuranceDbContext context)
    {
        _context = context;
    }

    public async Task<PolicyResponse?> GetPolicyByIdAsync(Guid id)
    {
        var policy = await _context.Policies.FindAsync(id);
        return policy != null ? MapToResponse(policy) : null;
    }

    public async Task<PagedResponse<PolicyResponse>> GetAllPoliciesAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Policies.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync();

        var policies = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<PolicyResponse>
        {
            Items = policies.Select(MapToResponse),
            Page = page,
            PageSize = pageSize,
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
