using PaymentService.Models.DTOs;

namespace PaymentService.Gateways;

public interface IPaymentGateway
{
    Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount);
    Task<GatewayResponse> CreatePaymentSessionAsync(GatewayChargeRequest request);
    string ProviderName { get; }
}