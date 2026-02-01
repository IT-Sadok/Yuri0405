using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class PolicyService : IPolicyService
{
    private readonly InsuranceDbContext _context;

    public PolicyService(InsuranceDbContext context)
    {
        _context = context;
    }

    public async Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request)
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Description = request.Description,
            ProductType = request.ProductType,
            CoverageAmount = request.CoverageAmount,
            PremiumAmount = request.PremiumAmount,
            DurationMonths = request.DurationMonths,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

        return MapToResponse(policy);
    }

    public async Task<PolicyResponse?> GetPolicyByIdAsync(Guid id)
    {
        var policy = await _context.Policies.FindAsync(id);
        return policy != null ? MapToResponse(policy) : null;
    }

    public async Task<IEnumerable<PolicyResponse>> GetAllPoliciesAsync()
    {
        var policies = await _context.Policies
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return policies.Select(MapToResponse);
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
            IsActive = policy.IsActive,
            CreatedAt = policy.CreatedAt
        };
    }
}
