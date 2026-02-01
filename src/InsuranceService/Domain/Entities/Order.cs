using Domain.Enums;

namespace Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid PolicyId { get; set; }
    public Policy Policy { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public OrderStatus Status { get; set; }
    public string? PaymentReferenceId { get; set; }
    public DateTime CreatedAt { get; set; }
}
