using PaymentService.Models.DTOs;

namespace PaymentService.Gateways;

public class MockGateway: IPaymentGateway
{
    public string ProviderName =>  "mock";

    public Task<GatewayResponse> ChargeAsync(GatewayChargeRequest paymentRequest)
    {
        // Simulate success 90% of the time
        var random = Random.Shared.Next(100);

        // 10% connection errors (throw exception)
        if (random < 10)
        {
            throw new HttpRequestException("Connection to gateway failed");
        }

        // 20% declined payments (return failure response)
        if (random < 30)
        {
            return Task.FromResult(new GatewayResponse
            {
                Success = false,
                ErrorMessage = "Insufficient funds",
            });
        }

            // 70% successful payments
        return Task.FromResult(new GatewayResponse
        {
            Success = true,
            ProviderPaymentId = $"mock_{Guid.NewGuid()}",
        });
            
    }

    public Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount)
    {
        throw new NotImplementedException();
    }
}