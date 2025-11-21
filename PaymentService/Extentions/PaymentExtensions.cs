using PaymentService.Models.DTOs;
using PaymentService.Models.Entities;

namespace PaymentService.Extentions;

public static class PaymentExtensions
{
    public static PaymentResponse ToResponse(this Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            IdempotencyKey = payment.IdempotencyKey,
            UserId = payment.UserId,
            PurchaseId = payment.PurchaseId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status.ToString().ToLower(),
            Message = payment.FailureReason,
            CreatedAt = payment.CreatedAt,
            CompletedAt = payment.CompletedAt
        };
    }
}
