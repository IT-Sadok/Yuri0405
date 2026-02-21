using Domain.Entities;

namespace Application.Interfaces;

public interface IPolicyRepository : IRepository<Policy>
{
    Task<(List<Policy> Items, int TotalCount)> GetAllAsync(int page, int pageSize, CancellationToken cancellationToken = default);
}
