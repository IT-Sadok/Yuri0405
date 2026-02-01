using Domain.Enums;

namespace Domain.Entities;

public class Policy
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ProductType ProductType { get; set; }
    public decimal CoverageAmount { get; set; }
    public decimal PremiumAmount { get; set; }
    public int DurationMonths { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}
