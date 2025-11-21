using PaymentService.Models.DTOs;

namespace PaymentService.Services;

public interface IPaymentService
{

    Task<PaymentSessionResponse> ProcessPaymentAsync(PaymentRequest request, string idempotencyKey);
    Task<PaymentResponse> GetPaymentAsync(Guid paymentId);
    Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync();
}