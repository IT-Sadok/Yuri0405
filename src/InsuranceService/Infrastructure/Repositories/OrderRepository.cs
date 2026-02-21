using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class OrderRepository(InsuranceDbContext context) : IOrderRepository
{
    public async Task<Order?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Orders.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Order entity, CancellationToken cancellationToken = default)
    {
        await context.Orders.AddAsync(entity, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<Order?> GetByIdWithPolicyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Orders
            .Include(o => o.Policy)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Orders
            .Include(o => o.Policy)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<(List<Order> Items, int TotalCount)> GetAllWithPolicyAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Orders
            .Include(o => o.Policy)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default)
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
}
