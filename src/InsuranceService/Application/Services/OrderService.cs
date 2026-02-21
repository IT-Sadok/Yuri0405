using Application.Interfaces;
using Domain.Enums;

namespace Application.Services;

public class OrderService(IOrderRepository orderRepository) : IOrderService
{
    public async Task<OrderActivationResult> ActivateOrderAsync(Guid orderId, string paymentReferenceId)
    {
        var order = await orderRepository.GetByIdAsync(orderId);

        if (order == null)
        {
            return OrderActivationResult.OrderNotFound;
        }

        if (order.Status == OrderStatus.Active)
        {
            return OrderActivationResult.AlreadyProcessed;
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            return OrderActivationResult.InvalidStatus;
        }

        order.Status = OrderStatus.Active;
        order.PaymentReferenceId = paymentReferenceId;

        await orderRepository.SaveChangesAsync();

        return OrderActivationResult.Success;
    }
}
