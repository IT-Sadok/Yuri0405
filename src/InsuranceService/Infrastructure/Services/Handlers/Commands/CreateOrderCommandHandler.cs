using Application.Commands;
using Application.DTOs;
using Application.Interfaces;
using Application.Mediator;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Handlers.Commands;

public class CreateOrderCommandHandler(InsuranceDbContext context, IPaymentService paymentService)
    : IRequestHandler<CreateOrderCommand, CreateOrderResponse>
{
    public async Task<CreateOrderResponse> Handle(CreateOrderCommand command, CancellationToken cancellationToken = default)
    {
        var request = command.Request;
        var customerId = command.CustomerId;

        var policy = await context.Policies.FindAsync([request.PolicyId], cancellationToken);
        if (policy == null)
        {
            throw new InvalidOperationException("Policy not found");
        }

        if (policy.Status != PolicyStatus.Active)
        {
            throw new InvalidOperationException("Policy is not active");
        }

        var orderNumber = await GenerateOrderNumberAsync(cancellationToken);
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(policy.DurationMonths);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            PolicyId = policy.Id,
            CustomerId = customerId,
            CustomerName = request.CustomerName,
            PremiumAmount = policy.PremiumAmount,
            StartDate = startDate,
            EndDate = endDate,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        context.Orders.Add(order);
        await context.SaveChangesAsync(cancellationToken);

        var paymentRequest = new InitiatePaymentRequest
        {
            OrderId = order.Id,
            Amount = order.PremiumAmount,
            Currency = request.Currency,
            Provider = request.Provider
        };

        var paymentResponse = await paymentService.InitiatePaymentAsync(paymentRequest);

        return new CreateOrderResponse
        {
            Order = MapToResponse(order, policy),
            CheckoutUrl = paymentResponse.CheckoutUrl,
            PaymentId = paymentResponse.PaymentId
        };
    }

    private async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken)
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"ORD-{currentYear}-";

        var orderNumbers = await context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .Select(o => o.OrderNumber)
            .ToListAsync(cancellationToken);

        if (!orderNumbers.Any())
        {
            return $"ORD-{currentYear}-001";
        }

        var maxNumber = orderNumbers
            .Select(on => int.TryParse(on.Split('-').Last(), out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"ORD-{currentYear}-{maxNumber + 1:D3}";
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
