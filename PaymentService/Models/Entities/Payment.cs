using PaymentService.Models.Enums;

namespace PaymentService.Models.Entities;

public class Payment
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; } = string.Empty;
    //Reference
    public Guid UserId { get; set; }
    public Guid? PurchaseId { get; set; }
    //Payment details
    public decimal Amount { get; set; }
    public string Currency { get; set; } = string.Empty;
    //Provider info
    public PaymentProvider ProviderId { get; set; }
    public string? ProviderPaymentId { get; set; }
    //Status
    public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
    public string? FailureReason { get; set; }
    //Timestamps
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? CompletedAt { get; set; } 
}