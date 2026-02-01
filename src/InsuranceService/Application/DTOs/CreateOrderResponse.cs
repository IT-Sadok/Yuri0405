namespace Application.DTOs;

public class CreateOrderResponse
{
    public OrderResponse Order { get; set; } = null!;
    public string? CheckoutUrl { get; set; }
    public Guid? PaymentId { get; set; }
}
