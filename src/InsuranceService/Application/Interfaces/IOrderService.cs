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
    Task<OrderActivationResult> ActivateOrderAsync(Guid orderId, string paymentReferenceId);
}
