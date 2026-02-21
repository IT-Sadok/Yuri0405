using Application.Commands;
using Application.Handlers.Commands;
using Domain.Enums;
using Infrastructure.Repositories;

namespace Tests.Commands;

public class PolicyCommandServiceTests
{
    [Fact]
    public async Task Handle_ValidCommand_CreatesPolicyAndReturnsIt()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var handler = new CreatePolicyCommandHandler(new PolicyRepository(context));
        var beforeCreate = DateTime.UtcNow;

        var command = new CreatePolicyCommand(
            Name: "Health Basic",
            Description: "Basic health coverage",
            ProductType: ProductType.Health,
            CoverageAmount: 50000m,
            PremiumAmount: 100m,
            DurationMonths: 12);

        // Act
        var result = await handler.Handle(command);
        var afterCreate = DateTime.UtcNow;

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(command.Name, result.Name);
        Assert.Equal(command.Description, result.Description);
        Assert.Equal(command.ProductType, result.ProductType);
        Assert.Equal(command.CoverageAmount, result.CoverageAmount);
        Assert.Equal(command.PremiumAmount, result.PremiumAmount);
        Assert.Equal(command.DurationMonths, result.DurationMonths);
        Assert.Equal(PolicyStatus.Active, result.Status);
        Assert.InRange(result.CreatedAt, beforeCreate, afterCreate);

        var savedPolicy = await context.Policies.FindAsync(result.Id);
        Assert.NotNull(savedPolicy);
        Assert.Equal(command.Name, savedPolicy.Name);
    }
}
