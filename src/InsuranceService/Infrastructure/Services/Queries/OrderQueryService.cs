using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Queries;

public class OrderQueryService : IOrderQueryService
{
    private readonly InsuranceDbContext _context;

    public OrderQueryService(InsuranceDbContext context)
    {
        _context = context;
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Policy)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order != null ? MapToResponse(order, order.Policy) : null;
    }

    public async Task<PagedResponse<OrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId, int page = 1, int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.Policy)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<OrderResponse>
        {
            Items = orders.Select(o => MapToResponse(o, o.Policy)),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<OrderResponse>> GetAllOrdersAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.Policy)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<OrderResponse>
        {
            Items = orders.Select(o => MapToResponse(o, o.Policy)),
            Page = page,
            PageSize = pageSize,
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
