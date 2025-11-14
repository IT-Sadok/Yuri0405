using PaymentService.Models.Enums;

namespace PaymentService.Models.DTOs;

public class PaymentRequest
{

    public Guid UserId { get; set; }
    public Guid? ProductId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentProvider Provider { get; set; }
}