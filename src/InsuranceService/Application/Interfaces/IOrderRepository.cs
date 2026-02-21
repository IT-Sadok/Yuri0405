using Domain.Entities;

namespace Application.Interfaces;

public interface IOrderRepository : IRepository<Order>
{
    Task<Order?> GetByIdWithPolicyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<(List<Order> Items, int TotalCount)> GetByCustomerIdAsync(Guid customerId, int page, int pageSize, CancellationToken cancellationToken = default);
    Task<(List<Order> Items, int TotalCount)> GetAllWithPolicyAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<string> GenerateOrderNumberAsync(CancellationToken cancellationToken = default);
}
