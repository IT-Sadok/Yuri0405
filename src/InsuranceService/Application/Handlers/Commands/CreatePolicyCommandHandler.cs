using Application.Commands;
using Application.DTOs;
using Application.Interfaces;
using Application.Mediator;
using Domain.Entities;
using Domain.Enums;

namespace Application.Handlers.Commands;

public class CreatePolicyCommandHandler(IPolicyRepository policyRepository)
    : IRequestHandler<CreatePolicyCommand, PolicyResponse>
{
    public async Task<PolicyResponse> Handle(CreatePolicyCommand command, CancellationToken cancellationToken = default)
    {
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = command.Name,
            Description = command.Description,
            ProductType = command.ProductType,
            CoverageAmount = command.CoverageAmount,
            PremiumAmount = command.PremiumAmount,
            DurationMonths = command.DurationMonths,
            Status = PolicyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await policyRepository.AddAsync(policy, cancellationToken);
        await policyRepository.SaveChangesAsync(cancellationToken);

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
