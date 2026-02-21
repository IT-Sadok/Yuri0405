using Application.DTOs;
using Application.Interfaces;
using Application.Mediator;
using Application.Queries;

namespace Application.Handlers.Queries;

public class GetOrderByIdQueryHandler(IOrderRepository orderRepository)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await orderRepository.GetByIdWithPolicyAsync(query.Id, cancellationToken);
        if (order == null) return null;

        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            PolicyId = order.PolicyId,
            PolicyName = order.Policy.Name,
            ProductType = order.Policy.ProductType,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            PremiumAmount = order.PremiumAmount,
            StartDate = order.StartDate,
            EndDate = order.EndDate,
            Status = order.Status,
            PaymentReferenceId = order.PaymentReferenceId,
            CreatedAt = order.CreatedAt
        };
    }
}
