using Application.Commands;
using Application.DTOs;
using Application.Handlers.Commands;
using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Repositories;
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
    public async Task CreateOrder_ValidCommand_ReturnsOrderWithCheckoutUrl()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var policy = CreateActivePolicy(context);
        await context.SaveChangesAsync();

        var handler = new CreateOrderCommandHandler(
            new OrderRepository(context),
            new PolicyRepository(context),
            _paymentServiceMock.Object);

        var command = new CreateOrderCommand(
            PolicyId: policy.Id,
            CustomerName: "John Doe",
            Currency: Currency.USD,
            Provider: PaymentProvider.Stripe,
            CustomerId: Guid.NewGuid());

        // Act
        var result = await handler.Handle(command);

        // Assert
        Assert.NotNull(result.Order);
        Assert.NotNull(result.CheckoutUrl);
        Assert.NotEqual(Guid.Empty, result.PaymentId);
        Assert.Equal(command.CustomerId, result.Order.CustomerId);
        Assert.Equal(OrderStatus.PendingPayment, result.Order.Status);
    }

    [Fact]
    public async Task CreateOrder_PolicyNotFound_ThrowsInvalidOperationException()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var handler = new CreateOrderCommandHandler(
            new OrderRepository(context),
            new PolicyRepository(context),
            _paymentServiceMock.Object);

        var command = new CreateOrderCommand(
            PolicyId: Guid.NewGuid(),
            CustomerName: "John Doe",
            Currency: Currency.USD,
            Provider: PaymentProvider.Stripe,
            CustomerId: Guid.NewGuid());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
    }

    [Fact]
    public async Task CreateOrder_InactivePolicy_ThrowsInvalidOperationException()
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

        var handler = new CreateOrderCommandHandler(
            new OrderRepository(context),
            new PolicyRepository(context),
            _paymentServiceMock.Object);

        var command = new CreateOrderCommand(
            PolicyId: policy.Id,
            CustomerName: "John Doe",
            Currency: Currency.USD,
            Provider: PaymentProvider.Stripe,
            CustomerId: Guid.NewGuid());

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => handler.Handle(command));
        Assert.Equal("Policy is not active", ex.Message);
    }

    [Fact]
    public async Task ActivateOrder_ValidOrder_ReturnsSuccess()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var order = CreateOrder(context);
        await context.SaveChangesAsync();

        var service = new OrderService(new OrderRepository(context));

        // Act
        var result = await service.ActivateOrderAsync(order.Id, "payment-ref-123");

        // Assert
        Assert.Equal(OrderActivationResult.Success, result);
        var updatedOrder = await context.Orders.FindAsync(order.Id);
        Assert.Equal(OrderStatus.Active, updatedOrder!.Status);
        Assert.Equal("payment-ref-123", updatedOrder.PaymentReferenceId);
    }

    [Fact]
    public async Task ActivateOrder_OrderNotFound_ReturnsOrderNotFound()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var service = new OrderService(new OrderRepository(context));

        // Act
        var result = await service.ActivateOrderAsync(Guid.NewGuid(), "payment-ref");

        // Assert
        Assert.Equal(OrderActivationResult.OrderNotFound, result);
    }

    [Fact]
    public async Task ActivateOrder_AlreadyActive_ReturnsAlreadyProcessed()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var order = CreateOrder(context, status: OrderStatus.Active);
        await context.SaveChangesAsync();

        var service = new OrderService(new OrderRepository(context));

        // Act
        var result = await service.ActivateOrderAsync(order.Id, "new-payment-ref");

        // Assert
        Assert.Equal(OrderActivationResult.AlreadyProcessed, result);
    }

    [Fact]
    public async Task ActivateOrder_CancelledOrder_ReturnsInvalidStatus()
    {
        // Arrange
        using var context = TestDbContextFactory.Create();
        var order = CreateOrder(context, status: OrderStatus.Cancelled);
        await context.SaveChangesAsync();

        var service = new OrderService(new OrderRepository(context));

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

    private static Order CreateOrder(
        Infrastructure.Data.InsuranceDbContext context,
        OrderStatus status = OrderStatus.PendingPayment)
    {
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
            Status = status,
            CreatedAt = DateTime.UtcNow
        };
        context.Orders.Add(order);
        return order;
    }
}
