using Domain.Enums;

namespace Application.DTOs;

public class CreateOrderRequest
{
    public Guid PolicyId { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public Currency Currency { get; set; }
    public PaymentProvider Provider { get; set; }
}
