using Application.DTOs;
using Application.Mediator;

namespace Application.Queries;

public record GetAllOrdersQuery(int Page, int PageSize) : IRequest<PagedResponse<OrderResponse>>;
