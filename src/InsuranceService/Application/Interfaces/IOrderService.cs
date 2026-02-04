using Application.DTOs;

namespace Application.Interfaces;

public enum OrderActivationResult
{
    Success,
    OrderNotFound,
    AlreadyProcessed,
    InvalidStatus
}

public interface IOrderService
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, Guid customerId);
    Task<OrderResponse?> GetOrderByIdAsync(Guid id);
    Task<PagedResponse<OrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId, int page = 1, int pageSize = 10);
    Task<PagedResponse<OrderResponse>> GetAllOrdersAsync(int page = 1, int pageSize = 10);
    Task<OrderActivationResult> ActivateOrderAsync(Guid orderId, string paymentReferenceId);
}
