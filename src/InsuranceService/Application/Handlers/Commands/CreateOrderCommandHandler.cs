using Application.Commands;
using Application.DTOs;
using Application.Interfaces;
using Application.Mediator;
using Domain.Entities;
using Domain.Enums;

namespace Application.Handlers.Commands;

public class CreateOrderCommandHandler(IOrderRepository orderRepository, IPolicyRepository policyRepository, IPaymentService paymentService)
    : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var policy = await policyRepository.GetByIdAsync(command.PolicyId, cancellationToken);
        if (policy == null)
        {
            throw new InvalidOperationException("Policy not found");
        }

        if (policy.Status != PolicyStatus.Active)
        {
            throw new InvalidOperationException("Policy is not active");
        }

        var orderNumber = await orderRepository.GenerateOrderNumberAsync(cancellationToken);
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(policy.DurationMonths);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            PolicyId = policy.Id,
            CustomerId = command.CustomerId,
            CustomerName = command.CustomerName,
            PremiumAmount = policy.PremiumAmount,
            StartDate = startDate,
            EndDate = endDate,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        await orderRepository.AddAsync(order, cancellationToken);
        await orderRepository.SaveChangesAsync(cancellationToken);

        var paymentRequest = new InitiatePaymentRequest
        {
            OrderId = order.Id,
            Amount = order.PremiumAmount,
            Currency = command.Currency,
            Provider = command.Provider
        };

        var paymentResponse = await paymentService.InitiatePaymentAsync(paymentRequest);

        return new CreateOrderResponse
        {
            Order = MapToResponse(order, policy),
            CheckoutUrl = paymentResponse.CheckoutUrl,
            PaymentId = paymentResponse.PaymentId
        };
    }

    private static OrderResponse MapToResponse(Order order, Policy policy)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            PolicyId = order.PolicyId,
            PolicyName = policy.Name,
            ProductType = policy.ProductType,
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
