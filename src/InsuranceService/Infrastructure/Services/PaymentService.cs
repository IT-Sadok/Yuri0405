using Application.DTOs;
using Application.Interfaces;

namespace Infrastructure.Services;

public class PaymentService(IPaymentHttpClient paymentHttpClient) : IPaymentService
{
    public async Task<PaymentInitiationResponse> InitiatePaymentAsync(InitiatePaymentRequest request)
    {
        var paymentRequest = new PaymentRequest
        {
            ProductId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            Provider = request.Provider
        };

        var paymentResponse = await paymentHttpClient.ProcessPaymentAsync(paymentRequest);

        return new PaymentInitiationResponse
        {
            Status = paymentResponse.Status,
            PaymentId = paymentResponse.PaymentId,
            CheckoutUrl = paymentResponse.Success ? paymentResponse.PaymentUrl : null
        };
    }
}
