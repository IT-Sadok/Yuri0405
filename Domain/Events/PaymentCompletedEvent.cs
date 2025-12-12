using Domain.Enums;

namespace Domain.Events;

public class PaymentCompletedEvent
{
    public Guid PaymentId { get; set; }
    public Guid? PurchaseId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public DateTime CompletedAt { get; set; }
}
