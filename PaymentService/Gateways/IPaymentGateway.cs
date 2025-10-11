using PaymentService.Models.DTOs;

namespace PaymentService.Gateways;

public interface IPaymentGateway
{
    Task<GatewayResponse> ChargeAsync(GatewayChargeRequest request);
    Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount);
    string ProviderName { get; }
}