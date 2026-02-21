using Application.DTOs;
using Application.Mediator;

namespace Application.Queries;

public record GetAllPoliciesQuery(int Page, int PageSize) : IRequest<PagedResponse<PolicyResponse>>;
