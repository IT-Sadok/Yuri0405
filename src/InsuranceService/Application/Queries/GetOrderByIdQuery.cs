using Application.DTOs;
using Application.Mediator;

namespace Application.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderResponse?>;
