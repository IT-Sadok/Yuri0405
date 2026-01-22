using Domain.Enums;

namespace Application.DTOs;

public class PolicyResponse
{
    public Guid Id { get; set; }
    public string PolicyNumber { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public PolicyStatus Status { get; set; }
    public string? PaymentReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}
