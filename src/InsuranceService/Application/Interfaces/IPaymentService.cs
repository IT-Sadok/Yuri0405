using Application.DTOs;

namespace Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentInitiationResponse> InitiatePaymentAsync(InitiatePaymentRequest request);
}
