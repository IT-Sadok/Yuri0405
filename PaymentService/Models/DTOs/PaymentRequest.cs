namespace PaymentService.Models.DTOs;

public class PaymentRequest
{
    
    public Guid UserId { get; set; }
    public Guid? ProductId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } // "USD", "EUR"
    public short ProviderId { get; set; }
}