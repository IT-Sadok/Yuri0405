using Application.DTOs;

namespace Application.Interfaces;

public interface IOrderQueryService
{
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<PagedResponse<OrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId, int page = 1, int pageSize = 10);
    Task<PagedResponse<OrderResponse>> GetAllOrdersAsync(int page = 1, int pageSize = 10);
}
