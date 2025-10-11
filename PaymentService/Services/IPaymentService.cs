using PaymentService.Models.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{
    
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, string idempotencyKey);
    Task<PaymentResponse> GetPaymentAsync(Guid paymentId);
}