using Application.DTOs;
using Domain.Enums;

namespace Application.Interfaces.Gateways;

public interface IPaymentGateway
{
    Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount);
    Task<GatewayResponse> CreatePaymentSessionAsync(GatewayChargeRequest request);
    PaymentProvider ProviderName { get; }
}
