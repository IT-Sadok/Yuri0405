using Application.DTOs;
using Application.Mediator;
using Application.Queries;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Handlers.Queries;

public class GetOrderByIdQueryHandler(InsuranceDbContext context)
    : IRequestHandler<GetOrderByIdQuery, OrderResponse?>
{
    public async Task<OrderResponse?> Handle(GetOrderByIdQuery query, CancellationToken cancellationToken = default)
    {
        var order = await context.Orders
            .Include(o => o.Policy)
            .FirstOrDefaultAsync(o => o.Id == query.Id, cancellationToken);

        return order != null ? MapToResponse(order, order.Policy) : null;
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
