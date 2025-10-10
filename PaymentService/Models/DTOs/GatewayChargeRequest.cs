namespace PaymentService.Models.DTOs;

public class GatewayChargeRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; }
    public string IdempotencyKey { get; set; }
}