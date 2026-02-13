using Application.DTOs;
using Application.Mediator;

namespace Application.Commands;

public record CreateOrderCommand(CreateOrderRequest Request, Guid CustomerId) : IRequest<CreateOrderResponse>;
