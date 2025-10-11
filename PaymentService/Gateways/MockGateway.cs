using PaymentService.Models.DTOs;

namespace PaymentService.Gateways;

public class MockGateway: IPaymentGateway
{
    public string ProviderName =>  "mock";

    public Task<GatewayResponse> ChargeAsync(GatewayChargeRequest paymentRequest)
    {
        // Simulate success 90% of the time
        var success = Random.Shared.Next(100) < 90;
        
        return Task.FromResult(new GatewayResponse
        {
            Success = success,
            ProviderPaymentId = success ? $"mock_{Guid.NewGuid()}" : null,
            ErrorMessage = success ? null : "Insufficient funds",
            Status = success ? "succeeded" : "failed"
        });
    }

    public Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount)
    {
        throw new NotImplementedException();
    }
}