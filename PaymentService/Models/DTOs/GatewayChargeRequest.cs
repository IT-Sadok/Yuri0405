using PaymentService.Models.Enums;

namespace PaymentService.Models.DTOs;

public class GatewayChargeRequest
{
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public string IdempotencyKey { get; set; }
    public string? PaymentToken { get; set; }
}