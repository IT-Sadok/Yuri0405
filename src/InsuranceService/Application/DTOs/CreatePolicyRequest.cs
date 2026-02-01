using Domain.Enums;

namespace Application.DTOs;

public class CreatePolicyRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal PremiumAmount { get; set; }
    public int DurationMonths { get; set; }
}
