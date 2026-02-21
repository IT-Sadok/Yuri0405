using Application.DTOs;
using Application.Mediator;

namespace Application.Queries;

public record GetPolicyByIdQuery(Guid Id) : IRequest<PolicyResponse?>;
