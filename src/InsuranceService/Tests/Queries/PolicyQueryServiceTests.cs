using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services.Queries;

namespace Tests.Queries;

public class PolicyQueryServiceTests
{
    [Fact]
    public async Task GetPolicyByIdAsync_ExistingPolicy_ReturnsPolicy()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "Test Policy",
            Description = "Test Description",
            ProductType = ProductType.Health,
            CoverageAmount = 50000m,
            PremiumAmount = 100m,
            DurationMonths = 12,
            Status = PolicyStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var service = new PolicyQueryService(context);

        // Act
        var result = await service.GetPolicyByIdAsync(policy.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(policy.Id, result.Id);
        Assert.Equal(policy.Name, result.Name);
    }

    [Fact]
    public async Task GetPolicyByIdAsync_NonExistingPolicy_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new PolicyQueryService(context);

        // Act
        var result = await service.GetPolicyByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllPoliciesAsync_ReturnsPaginatedResultsOrderedByNewest()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        for (int i = 0; i < 15; i++)
        {
            context.Policies.Add(new Policy
            {
                Id = Guid.NewGuid(),
                Name = $"Policy {i}",
                Description = $"Description {i}",
                ProductType = ProductType.Health,
                CoverageAmount = 50000m,
                PremiumAmount = 100m,
                DurationMonths = 12,
                Status = PolicyStatus.Active,
                CreatedAt = DateTime.UtcNow.AddMinutes(-i)
            });
        }
        await context.SaveChangesAsync();

        var service = new PolicyQueryService(context);

        // Act
        var result = await service.GetAllPoliciesAsync(page: 1, pageSize: 10);

        // Assert
        Assert.Equal(10, result.Items.Count());
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(1, result.Page);
        Assert.Equal(10, result.PageSize);

        var items = result.Items.ToList();
        Assert.Equal("Policy 0", items[0].Name);
        Assert.Equal("Policy 9", items[9].Name);
    }
}
