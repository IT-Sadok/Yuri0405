using PaymentService.Models.DTOs;
using PaymentService.Models.Enums;

namespace PaymentService.Gateways;

public class MockGateway: IPaymentGateway
{
    public PaymentProvider ProviderName => PaymentProvider.Mock;

    public Task<GatewayResponse> CreatePaymentSessionAsync(GatewayChargeRequest request)
    {
        // Simulate success 90% of the time
        var random = Random.Shared.Next(100);

        // 10% connection errors (throw exception)
        if (random < 10)
        {
            throw new HttpRequestException("Mock gateway connection failed");
        }

        // 20% session creation failures (return failure response)
        if (random < 30)
        {
            return Task.FromResult(new GatewayResponse
            {
                Success = false,
                ErrorMessage = "Mock payment session creation failed: Invalid request parameters",
            });
        }

        // 70% successful session creation
        var sessionId = $"mock_session_{Guid.NewGuid()}";
        return Task.FromResult(new GatewayResponse
        {
            Success = true,
            ProviderPaymentId = sessionId,
            RedirectUrl = $"http://localhost:5000/mock-checkout/{sessionId}",
        });
    }

    public Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount)
    {
        throw new NotImplementedException();
    }
}