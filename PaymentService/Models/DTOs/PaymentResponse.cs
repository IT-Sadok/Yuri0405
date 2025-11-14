using PaymentService.Models.Enums;

namespace PaymentService.Models.DTOs;

public class PaymentResponse
{
    public Guid Id { get; set; }
    public string IdempotencyKey { get; set; }
    public Guid UserId { get; set; }
    public Guid? PurchaseId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public string Provider { get; set; }
    public string Status { get; set; } // "pending", "completed", "failed"
    public string? Message { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}