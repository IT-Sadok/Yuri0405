using Application.DTOs;
using Application.Mediator;
using Domain.Enums;

namespace Application.Commands;

public record CreateOrderCommand(
    Guid PolicyId,
    string CustomerName,
    Currency Currency,
    PaymentProvider Provider,
    Guid CustomerId = default) : IRequest<CreateOrderResponse>;
