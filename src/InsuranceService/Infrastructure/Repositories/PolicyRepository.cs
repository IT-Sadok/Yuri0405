using Application.Interfaces;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories;

public class PolicyRepository(InsuranceDbContext context) : IPolicyRepository
{
    public async Task<Policy?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Policies.FindAsync([id], cancellationToken);
    }

    public async Task AddAsync(Policy entity, CancellationToken cancellationToken = default)
    {
        await context.Policies.AddAsync(entity, cancellationToken);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(List<Policy> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var query = context.Policies.OrderByDescending(p => p.CreatedAt);

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }
}
