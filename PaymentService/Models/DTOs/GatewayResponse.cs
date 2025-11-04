namespace PaymentService.Models.DTOs;

public class GatewayResponse
{
    public bool Success { get; set; }
    public string? ProviderPaymentId { get; set; }
    public string? ErrorMessage { get; set; }
}