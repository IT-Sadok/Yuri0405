using Application.DTOs;
using Application.Mediator;
using Application.Queries;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Handlers.Queries;

public class GetOrdersByCustomerIdQueryHandler(InsuranceDbContext context)
    : IRequestHandler<GetOrdersByCustomerIdQuery, PagedResponse<OrderResponse>>
{
    public async Task<PagedResponse<OrderResponse>> Handle(GetOrdersByCustomerIdQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = context.Orders
            .Include(o => o.Policy)
            .Where(o => o.CustomerId == query.CustomerId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var orders = await dbQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        return new PagedResponse<OrderResponse>
        {
            Items = orders.Select(o => MapToResponse(o, o.Policy)),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
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
