using Application.DTOs;
using Application.Mediator;
using Domain.Enums;

namespace Application.Commands;

public record CreatePolicyCommand(
    string Name,
    string Description,
    ProductType ProductType,
    decimal CoverageAmount,
    decimal PremiumAmount,
    int DurationMonths) : IRequest<PolicyResponse>;
