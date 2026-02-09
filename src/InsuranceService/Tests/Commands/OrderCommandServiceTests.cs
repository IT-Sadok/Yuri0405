using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Services.Commands;
using Moq;

namespace Tests.Commands;

public class OrderCommandServiceTests
{
    private readonly Mock<IPaymentService> _paymentServiceMock;

    public OrderCommandServiceTests()
    {
        _paymentServiceMock = new Mock<IPaymentService>();
        _paymentServiceMock
            .Setup(x => x.InitiatePaymentAsync(It.IsAny<InitiatePaymentRequest>()))
            .ReturnsAsync(new PaymentInitiationResponse
            {
                PaymentId = Guid.NewGuid(),
                CheckoutUrl = "https://payment.example.com/checkout",
                Status = "pending"
            });
    }

    [Fact]
    public async Task CreateOrderAsync_ValidRequest_ReturnsOrderWithCheckoutUrl()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var policy = CreateActivePolicy(context);
        await context.SaveChangesAsync();

        var service = new OrderCommandService(context, _paymentServiceMock.Object);
        var customerId = Guid.NewGuid();

        var request = new CreateOrderRequest
        {
            PolicyId = policy.Id,
            CustomerName = "John Doe",
            Currency = Currency.USD,
            Provider = PaymentProvider.Stripe
        };

        // Act
        var result = await service.CreateOrderAsync(request, customerId);

        // Assert
        Assert.NotNull(result.Order);
        Assert.NotNull(result.CheckoutUrl);
        Assert.NotEqual(Guid.Empty, result.PaymentId);
        Assert.Equal(customerId, result.Order.CustomerId);
        Assert.Equal(OrderStatus.PendingPayment, result.Order.Status);
    }

    [Fact]
    public async Task CreateOrderAsync_PolicyNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new OrderCommandService(context, _paymentServiceMock.Object);

        var request = new CreateOrderRequest
        {
            PolicyId = Guid.NewGuid(),
            CustomerName = "John Doe",
            Currency = Currency.USD,
            Provider = PaymentProvider.Stripe
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateOrderAsync(request, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateOrderAsync_InactivePolicy_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var policy = new Policy
        {
            Id = Guid.NewGuid(),
            Name = "Inactive Policy",
            Description = "Test",
            ProductType = ProductType.Health,
            CoverageAmount = 50000m,
            PremiumAmount = 100m,
            DurationMonths = 12,
            Status = PolicyStatus.Inactive,
            CreatedAt = DateTime.UtcNow
        };
        context.Policies.Add(policy);
        await context.SaveChangesAsync();

        var service = new OrderCommandService(context, _paymentServiceMock.Object);

        var request = new CreateOrderRequest
        {
            PolicyId = policy.Id,
            CustomerName = "John Doe",
            Currency = Currency.USD,
            Provider = PaymentProvider.Stripe
        };

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CreateOrderAsync(request, Guid.NewGuid()));
        Assert.Equal("Policy is not active", exception.Message);
    }

    [Fact]
    public async Task ActivateOrderAsync_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-001",
            PolicyId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PremiumAmount = 100m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(12),
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderCommandService(context, _paymentServiceMock.Object);

        // Act
        var result = await service.ActivateOrderAsync(order.Id, "payment-ref-123");

        // Assert
        Assert.Equal(OrderActivationResult.Success, result);

        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Active, updatedOrder!.Status);
        Assert.Equal("payment-ref-123", updatedOrder.PaymentReferenceId);
    }

    [Fact]
    public async Task ActivateOrderAsync_OrderNotFound_ReturnsOrderNotFound()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new OrderCommandService(context, _paymentServiceMock.Object);

        // Act
        var result = await service.ActivateOrderAsync(Guid.NewGuid(), "payment-ref");

        // Assert
        Assert.Equal(OrderActivationResult.OrderNotFound, result);
    }

    [Fact]
    public async Task ActivateOrderAsync_AlreadyActive_ReturnsAlreadyProcessed()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-001",
            PolicyId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PremiumAmount = 100m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(12),
            Status = OrderStatus.Active,
            PaymentReferenceId = "existing-ref",
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderCommandService(context, _paymentServiceMock.Object);

        // Act
        var result = await service.ActivateOrderAsync(order.Id, "new-payment-ref");

        // Assert
        Assert.Equal(OrderActivationResult.AlreadyProcessed, result);
    }

    [Fact]
    public async Task ActivateOrderAsync_CancelledOrder_ReturnsInvalidStatus()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-2026-001",
            PolicyId = Guid.NewGuid(),
            CustomerId = Guid.NewGuid(),
            CustomerName = "John Doe",
            PremiumAmount = 100m,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddMonths(12),
            Status = OrderStatus.Cancelled,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        await context.SaveChangesAsync();

        var service = new OrderCommandService(context, _paymentServiceMock.Object);

        // Act
        var result = await service.ActivateOrderAsync(order.Id, "payment-ref");

        // Assert
        Assert.Equal(OrderActivationResult.InvalidStatus, result);
    }

    private static Policy CreateActivePolicy(Infrastructure.Data.InsuranceDbContext context)
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
}
