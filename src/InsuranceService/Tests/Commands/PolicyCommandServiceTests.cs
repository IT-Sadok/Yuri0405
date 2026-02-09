using Application.DTOs;
using Domain.Enums;
using Infrastructure.Services.Commands;

namespace Tests.Commands;

public class PolicyCommandServiceTests
{
    [Fact]
    public async Task CreatePolicyAsync_ValidRequest_CreatesPolicyAndReturnsIt()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new PolicyCommandService(context);
        var beforeCreate = DateTime.UtcNow;

        var request = new CreatePolicyRequest
        {
            Name = "Health Basic",
            Description = "Basic health coverage",
            ProductType = ProductType.Health,
            CoverageAmount = 50000m,
            PremiumAmount = 100m,
            DurationMonths = 12
        };

        // Act
        var result = await service.CreatePolicyAsync(request);
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.ProductType, result.ProductType);
        Assert.Equal(request.CoverageAmount, result.CoverageAmount);
        Assert.Equal(request.PremiumAmount, result.PremiumAmount);
        Assert.Equal(request.DurationMonths, result.DurationMonths);
        Assert.Equal(PolicyStatus.Active, result.Status);
        Assert.InRange(result.CreatedAt, beforeCreate, afterCreate);

        var savedPolicy = await context.Policies.FindAsync(result.Id);
        Assert.NotNull(savedPolicy);
        Assert.Equal(request.Name, savedPolicy.Name);
    }
}
