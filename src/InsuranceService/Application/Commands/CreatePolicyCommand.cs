using Application.DTOs;
using Application.Mediator;

namespace Application.Commands;

public record CreatePolicyCommand(CreatePolicyRequest Request) : IRequest<PolicyResponse>;
