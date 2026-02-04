using Application.DTOs;
using Application.Interfaces;
using Domain.Entities;
using Domain.Enums;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services;

public class OrderService : IOrderService
{
    private readonly InsuranceDbContext _context;
    private readonly IPaymentService _paymentService;

    public OrderService(InsuranceDbContext context, IPaymentService paymentService)
    {
        _context = context;
        _paymentService = paymentService;
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, Guid customerId)
    {
        var policy = await _context.Policies.FindAsync(request.PolicyId);
        if (policy == null)
        {
            throw new InvalidOperationException("Policy not found");
        }

        if (!policy.IsActive)
        {
            throw new InvalidOperationException("Policy is not active");
        }

        var orderNumber = await GenerateOrderNumberAsync();
        var startDate = DateTime.UtcNow;
        var endDate = startDate.AddMonths(policy.DurationMonths);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            PolicyId = policy.Id,
            CustomerId = customerId,
            CustomerName = request.CustomerName,
            PremiumAmount = policy.PremiumAmount,
            StartDate = startDate,
            EndDate = endDate,
            Status = OrderStatus.PendingPayment,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        var paymentRequest = new InitiatePaymentRequest
        {
            OrderId = order.Id,
            Amount = order.PremiumAmount,
            Currency = request.Currency,
            Provider = request.Provider
        };

        var paymentResponse = await _paymentService.InitiatePaymentAsync(paymentRequest);

        return new CreateOrderResponse
        {
            Order = MapToResponse(order, policy),
            CheckoutUrl = paymentResponse.CheckoutUrl,
            PaymentId = paymentResponse.PaymentId
        };
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(Guid id)
    {
        var order = await _context.Orders
            .Include(o => o.Policy)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order != null ? MapToResponse(order, order.Policy) : null;
    }

    public async Task<PagedResponse<OrderResponse>> GetOrdersByCustomerIdAsync(Guid customerId, int page = 1, int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.Policy)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<OrderResponse>
        {
            Items = orders.Select(o => MapToResponse(o, o.Policy)),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PagedResponse<OrderResponse>> GetAllOrdersAsync(int page = 1, int pageSize = 10)
    {
        var query = _context.Orders
            .Include(o => o.Policy)
            .OrderByDescending(o => o.CreatedAt);

        var totalCount = await query.CountAsync();

        var orders = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResponse<OrderResponse>
        {
            Items = orders.Select(o => MapToResponse(o, o.Policy)),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<OrderActivationResult> ActivateOrderAsync(Guid orderId, string paymentReferenceId)
    {
        var order = await _context.Orders.FindAsync(orderId);

        if (order == null)
        {
            return OrderActivationResult.OrderNotFound;
        }

        if (order.Status == OrderStatus.Active)
        {
            return OrderActivationResult.AlreadyProcessed;
        }

        if (order.Status != OrderStatus.PendingPayment)
        {
            return OrderActivationResult.InvalidStatus;
        }

        order.Status = OrderStatus.Active;
        order.PaymentReferenceId = paymentReferenceId;

        await _context.SaveChangesAsync();

        return OrderActivationResult.Success;
    }

    private async Task<string> GenerateOrderNumberAsync()
    {
        var currentYear = DateTime.UtcNow.Year;
        var prefix = $"ORD-{currentYear}-";

        var orderNumbers = await _context.Orders
            .Where(o => o.OrderNumber.StartsWith(prefix))
            .Select(o => o.OrderNumber)
            .ToListAsync();

        if (!orderNumbers.Any())
        {
            return $"ORD-{currentYear}-001";
        }

        var maxNumber = orderNumbers
            .Select(on => int.TryParse(on.Split('-').Last(), out var num) ? num : 0)
            .DefaultIfEmpty(0)
            .Max();

        return $"ORD-{currentYear}-{maxNumber + 1:D3}";
    }

    private static OrderResponse MapToResponse(Order order, Policy policy)
    {
        return new OrderResponse
        {
            Id = order.Id,
            OrderNumber = order.OrderNumber,
            PolicyId = order.PolicyId,
            PolicyName = policy.Name,
            ProductType = policy.ProductType,
            CustomerId = order.CustomerId,
            CustomerName = order.CustomerName,
            PremiumAmount = order.PremiumAmount,
            StartDate = order.StartDate,
            EndDate = order.EndDate,
            Status = order.Status,
            PaymentReferenceId = order.PaymentReferenceId,
            CreatedAt = order.CreatedAt
        };
    }
}
