using Application.DTOs;
using Application.Interfaces;
using Infrastructure.Data;

namespace Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly InsuranceDbContext _context;
    private readonly IPaymentHttpClient _paymentHttpClient;

    public PaymentService(InsuranceDbContext context, IPaymentHttpClient paymentHttpClient)
    {
        _context = context;
        _paymentHttpClient = paymentHttpClient;
    }

    public async Task<PaymentInitiationResponse> InitiatePaymentAsync(InitiatePaymentRequest request, string jwtToken)
    {
        var policy = await _context.Policies.FindAsync(request.PolicyId);
        if (policy == null)
        {
            throw new InvalidOperationException("Policy not found");
        }

        var paymentRequest = new PaymentRequest
        {
            ProductId = request.PolicyId,
            Amount = request.Amount,
            Currency = request.Currency,
            Provider = request.Provider
        };

        var paymentResponse = await _paymentHttpClient.ProcessPaymentAsync(paymentRequest, jwtToken);

        if (paymentResponse.Success)
        {
            policy.PaymentReferenceId = paymentResponse.PaymentId.ToString();
            await _context.SaveChangesAsync();
        }

        return new PaymentInitiationResponse
        {
            Status = paymentResponse.Status,
            PaymentId = paymentResponse.PaymentId,
            CheckoutUrl = paymentResponse.Success ? paymentResponse.PaymentUrl : null
        };
    }
}
