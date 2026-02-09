using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;

namespace Infrastructure.Services.Commands;

public class PolicyCommandService : IPolicyCommandService
{
    private readonly InsuranceDbContext _context;

    public PolicyCommandService(InsuranceDbContext context)
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
            Status = PolicyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        _context.Policies.Add(policy);
        await _context.SaveChangesAsync();

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
