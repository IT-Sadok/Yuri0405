using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
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

    public async Task<PolicyResponse> CreatePolicyAsync(CreatePolicyRequest request, Guid customerId)
    {
        var policyNumber = await GeneratePolicyNumberAsync();

        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(request.DurationMonths);

        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            PolicyNumber = policyNumber,
            ProductType = request.ProductType,
            CustomerId = customerId,
            CustomerName = request.CustomerName,
            PremiumAmount = request.PremiumAmount,
            StartDate = startDate,
            EndDate = endDate,
            Status = PolicyStatus.PendingPayment,
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

    public async Task<IEnumerable<PolicyResponse>> GetPoliciesByCustomerIdAsync(Guid customerId)
    {
        var policies = await _context.Policies
            .Where(p => p.CustomerId == customerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return policies.Select(p => MapToResponse(p));
    }

    public async Task<IEnumerable<PolicyResponse>> GetAllPoliciesAsync()
    {
        var policies = await _context.Policies
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return policies.Select(p => MapToResponse(p));
    }

    private async Task<string> GeneratePolicyNumberAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"POL-{currentYear}-";

        var policyNumbers = await _context.Policies
            .Where(p => p.PolicyNumber.StartsWith(prefix))
            .Select(p => p.PolicyNumber)
            .ToListAsync();

        if (!policyNumbers.Any())
        {
            return $"POL-{currentYear}-001";
        }

        var maxNumber = policyNumbers
            .Select(pn => int.TryParse(pn.Split('-').Last(), out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"POL-{currentYear}-{maxNumber + 1:D3}";
    }

    private static PolicyResponse MapToResponse(Policy policy)
    {
        return new PolicyResponse
        {
            Id = policy.Id,
            PolicyNumber = policy.PolicyNumber,
            ProductType = policy.ProductType,
            CustomerId = policy.CustomerId,
            CustomerName = policy.CustomerName,
            PremiumAmount = policy.PremiumAmount,
            StartDate = policy.StartDate,
            EndDate = policy.EndDate,
            Status = policy.Status,
            PaymentReferenceId = policy.PaymentReferenceId,
            CreatedAt = policy.CreatedAt
        };
    }
}
