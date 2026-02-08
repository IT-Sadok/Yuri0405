namespace Application.DTOs;

public class PaymentSessionResponse
{
    public Guid PaymentId { get; set; }
    public string PaymentUrl { get; set; }
    public string Status { get; set; }
}
