using Application.Interfaces;
using Domain.Enums;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class OrderService(InsuranceDbContext context) : IOrderService
{
    public async Task<OrderActivationResult> ActivateOrderAsync(Guid orderId, string paymentReferenceId)
    {
        var order = await context.Orders.FindAsync(orderId);

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

        await context.SaveChangesAsync();

        return OrderActivationResult.Success;
    }
}
