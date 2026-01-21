using Domain.Enums;

namespace Application.Interfaces;

public interface IPaymentHttpClient
{
    Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest request, string jwtToken);
}

public class PaymentRequest
{
    public Guid? ProductId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentProvider Provider { get; set; }
}

public class PaymentResponse
{
    public Guid PaymentId { get; set; }
    public string PaymentUrl { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public bool Success { get; set; }
}
