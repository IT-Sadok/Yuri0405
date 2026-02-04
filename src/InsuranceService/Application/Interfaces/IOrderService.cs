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
    Task<IEnumerable<OrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId);
    Task<IEnumerable<OrderResponse>> GetAllOrdersAsync();
    Task<OrderActivationResult> ActivateOrderAsync(Guid orderId, string paymentReferenceId);
}
