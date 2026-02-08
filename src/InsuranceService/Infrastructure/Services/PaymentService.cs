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

    public async Task<PaymentInitiationResponse> InitiatePaymentAsync(InitiatePaymentRequest request)
    {
        var order = await _context.Orders.FindAsync(request.OrderId);
        if (order == null)
        {
            throw new InvalidOperationException("Order not found");
        }

        var paymentRequest = new PaymentRequest
        {
            ProductId = request.OrderId,
            Amount = request.Amount,
            Currency = request.Currency,
            Provider = request.Provider
        };

        var paymentResponse = await _paymentHttpClient.ProcessPaymentAsync(paymentRequest);

        if (paymentResponse.Success)
        {
            order.PaymentReferenceId = paymentResponse.PaymentId.ToString();
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
