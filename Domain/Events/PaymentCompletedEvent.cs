using Domain.Enums;

namespace Domain.Events;

public class PaymentCompletedEvent: BaseDomainEvent
{
    public Guid PaymentId { get; set; }
    public Guid? PurchaseId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
}
