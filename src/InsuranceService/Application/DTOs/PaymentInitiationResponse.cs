namespace Application.DTOs;

public class PaymentInitiationResponse
{
    public string Status { get; set; } = string.Empty;
    public Guid PaymentId { get; set; }
    public string? CheckoutUrl { get; set; }
}
