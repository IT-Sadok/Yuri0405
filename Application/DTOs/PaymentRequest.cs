using System.Text.Json.Serialization;
using Domain.Enums;

namespace Application.DTOs;

public class PaymentRequest
{
    public Guid? ProductId { get; set; }
    public decimal Amount { get; set; }
    public Currency Currency { get; set; }
    public PaymentProvider Provider { get; set; }
}
