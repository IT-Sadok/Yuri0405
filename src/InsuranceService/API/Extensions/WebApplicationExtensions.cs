using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions;

public static class WebApplicationExtensions
{
    public static WebApplication MigrateAndSeedDatabase(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<InsuranceDbContext>();

        db.Database.Migrate();

        if (!db.Policies.Any())
        {
            db.Policies.AddRange(
                new Policy
                {
                    Id = Guid.NewGuid(),
                    Name = "Basic Health Plan",
                    Description = "Essential health coverage for individuals including doctor visits and emergency care",
                    ProductType = ProductType.Health,
                    CoverageAmount = 50_000m,
                    PremiumAmount = 199.99m,
                    DurationMonths = 12,
                    Status = PolicyStatus.Active,
                    CreatedAt = DateTime.UtcNow
                },
                new Policy
                {
                    Id = Guid.NewGuid(),
                    Name = "Full Auto Coverage",
                    Description = "Comprehensive auto insurance covering collision, liability, and uninsured motorist",
                    ProductType = ProductType.Auto,
                    CoverageAmount = 100_000m,
                    PremiumAmount = 89.50m,
                    DurationMonths = 6,
                    Status = PolicyStatus.Active,
                    CreatedAt = DateTime.UtcNow
                },
                new Policy
                {
                    Id = Guid.NewGuid(),
                    Name = "Home Protection Plus",
                    Description = "Full home insurance covering structure, personal property, and natural disasters",
                    ProductType = ProductType.Home,
                    CoverageAmount = 250_000m,
                    PremiumAmount = 149.00m,
                    DurationMonths = 12,
                    Status = PolicyStatus.Active,
                    CreatedAt = DateTime.UtcNow
                }
            );
            db.SaveChanges();
        }

        return app;
    }
}
