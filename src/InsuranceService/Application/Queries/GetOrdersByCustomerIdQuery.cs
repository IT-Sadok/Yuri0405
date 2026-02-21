using Application.DTOs;
using Application.Mediator;

namespace Application.Queries;

public record GetOrdersByCustomerIdQuery(Guid CustomerId, int Page, int PageSize) : IRequest<PagedResponse<OrderResponse>>;
