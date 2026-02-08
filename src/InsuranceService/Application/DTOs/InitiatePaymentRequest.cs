using Domain.Enums;

namespace Application.DTOs;

public class InitiatePaymentRequest
{
    public Guid OrderId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentProvider Provider { get; set; }
}
