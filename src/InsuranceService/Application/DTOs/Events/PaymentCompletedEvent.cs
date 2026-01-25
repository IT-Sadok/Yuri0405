namespace Application.DTOs.Events;

public class PaymentCompletedEvent
{
    public Guid Id { get; set; }
    public DateTime OccuredOn { get; set; }
    public Guid PaymentId { get; set; }
    public Guid PurchaseId { get; set; }
    public decimal Amount { get; set; }
    public int Currency { get; set; }
}
