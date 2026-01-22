using Domain.Enums;

namespace Application.DTOs;

public class CreatePolicyRequest
{
    public ProductType ProductType { get; set; }
    public string CustomerName { get; set; } = string.Empty;
    public decimal PremiumAmount { get; set; }
    public int DurationMonths { get; set; }
}
