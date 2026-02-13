using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services.Queries;

namespace Tests.Queries;

public class OrderQueryServiceTests
{
    [Fact]
    public async Task GetOrderByIdAsync_ExistingOrder_ReturnsOrderWithPolicy()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var policy = CreatePolicy(context);
        var order = CreateOrder(context, policy);
        await context.SaveChangesAsync();

        var service = new OrderQueryService(context);

        // Act
        var result = await service.GetOrderByIdAsync(order.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(order.Id, result.Id);
        Assert.Equal(order.OrderNumber, result.OrderNumber);
        Assert.Equal(policy.Name, result.PolicyName);
        Assert.Equal(policy.ProductType, result.ProductType);
    }

    [Fact]
    public async Task GetOrderByIdAsync_NonExistingOrder_ReturnsNull()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new OrderQueryService(context);

        // Act
        var result = await service.GetOrderByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrdersByCustomerIdAsync_ReturnsOnlyCustomerOrders()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var policy = CreatePolicy(context);
        var customerId = Guid.NewGuid();
        var otherCustomerId = Guid.NewGuid();

        for (int i = 0; i < 3; i++)
        {
            CreateOrder(context, policy, customerId, $"ORD-2026-{100 + i}");
        }

        for (int i = 0; i < 2; i++)
        {
            CreateOrder(context, policy, otherCustomerId, $"ORD-2026-{200 + i}");
        }

        await context.SaveChangesAsync();

        var service = new OrderQueryService(context);

        // Act
        var result = await service.GetOrdersByCustomerIdAsync(customerId);

        // Assert
        Assert.Equal(3, result.Items.Count());
        Assert.Equal(3, result.TotalCount);
        Assert.All(result.Items, o => Assert.Equal(customerId, o.CustomerId));
    }

    [Fact]
    public async Task GetAllOrdersAsync_ReturnsPaginatedResults()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var policy = CreatePolicy(context);

        for (int i = 0; i < 15; i++)
        {
            CreateOrder(context, policy, Guid.NewGuid(), $"ORD-2026-{i:D3}");
        }
        await context.SaveChangesAsync();

        var service = new OrderQueryService(context);

        // Act
        var result = await service.GetAllOrdersAsync(page: 1, pageSize: 10);

        // Assert
        Assert.Equal(10, result.Items.Count());
        Assert.Equal(15, result.TotalCount);
    }

    private static Policy CreatePolicy(Infrastructure.Data.InsuranceDbContext context)
    {
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
        return policy;
    }

    private static Order CreateOrder(
        Infrastructure.Data.InsuranceDbContext context,
        Policy policy,
        Guid? customerId = null,
        string? orderNumber = null)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber ?? $"ORD-2026-{Guid.NewGuid().ToString()[..3]}",
            PolicyId = policy.Id,
            Policy = policy,
            CustomerId = customerId ?? Guid.NewGuid(),
            CustomerName = "Test Customer",
            PremiumAmount = policy.PremiumAmount,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(policy.DurationMonths),
            Status = OrderStatus.Active,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        return order;
    }
}
