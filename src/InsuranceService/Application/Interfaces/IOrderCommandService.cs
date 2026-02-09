using Application.DTOs;

namespace Application.Interfaces;

public enum OrderActivationResult
{
    Success,
    OrderNotFound,
    AlreadyProcessed,
    InvalidStatus
}

public interface IOrderCommandService
{
    Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, Guid customerId);
    Task<OrderActivationResult> ActivateOrderAsync(Guid orderId, string paymentReferenceId);
}
