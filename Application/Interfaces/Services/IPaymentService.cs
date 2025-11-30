using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IPaymentService
{

    Task<PaymentSessionResponse> ProcessPaymentAsync(PaymentRequest request, string idempotencyKey);
    Task<PaymentResponse> GetPaymentAsync(Guid paymentId);
    Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync();
}
