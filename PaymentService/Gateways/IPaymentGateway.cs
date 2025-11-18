using PaymentService.Models.DTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Gateways;

public interface IPaymentGateway
{
    Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount);
    Task<GatewayResponse> CreatePaymentSessionAsync(GatewayChargeRequest request);
    PaymentProvider ProviderName { get; }
}