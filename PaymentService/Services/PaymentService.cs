using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Gateways;
using PaymentService.Models.DTOs;
using PaymentService.Models.Entities;
using PaymentService.Models.Enums;

namespace PaymentService.Services;

public class PaymentService: IPaymentService
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentDbContext _dbContext;

    public PaymentService(
        IPaymentGatewayFactory paymentGatewayFactory, ILogger<PaymentService> logger,
        PaymentDbContext dbContext)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<PaymentResponse> ProcessPaymentAsync(PaymentRequest paymentRequest, string idempotencyKey)
    {
        //Checking existing payment
        var existing = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);
        if (existing != null)
        {
            _logger.LogInformation(
                "Returning cached payment for idempotency key {Key}",idempotencyKey);
            return MapToPresponse(existing);
        }
        
        //Create and save payment before calling gateway
        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            IdempotencyKey = idempotencyKey,
            UserId = paymentRequest.UserId,
            PurchaseId = paymentRequest.ProductId,
            Amount = paymentRequest.Amount,
            Currency = paymentRequest.Currency,
            ProviderId = paymentRequest.ProviderId,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _dbContext.Payments.AddAsync(payment);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation(
            "Created payment {PaymentId} with status {Status}",
            payment.Id, payment.Status.ToString());
        
        //Update status to processing and call gateway
        payment.Status = PaymentStatus.Processing;
        payment.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();

        var gateway = _paymentGatewayFactory.GetGateway(paymentRequest.ProviderId);
        var gatewayRequest = new GatewayChargeRequest
        {
            Amount = payment.Amount,
            Currency = payment.Currency,
            IdempotencyKey = idempotencyKey
        };
        GatewayResponse result;
        try
        {
            result = await gateway.ChargeAsync(gatewayRequest);
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Gateway call failed for payment {PaymentId}",
                payment.Id);
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Gateway communication failure";
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            throw;
        }

        payment.ProviderPaymentId = result.ProviderPaymentId;
        payment.Status = result.Success ? PaymentStatus.Completed : PaymentStatus.Failed;
        payment.FailureReason = result.ErrorMessage;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.CompletedAt = result.Success ? DateTime.UtcNow : null;
        
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation(
            "Payment {PaymentId} completed with status {Status}",
            payment.Id, payment.Status.ToString());
        
        return MapToPresponse(payment);
    }

    public async Task<PaymentResponse> GetPaymentAsync(Guid paymentId)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null)
            throw new KeyNotFoundException($"Payment {paymentId} not found");
        return MapToPresponse(payment);
    }

    public async Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync()
    {
        var payments = await _dbContext.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(MapToPresponse);
    }

    private PaymentResponse MapToPresponse( Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            IdempotencyKey = payment.IdempotencyKey,
            UserId = payment.UserId,
            PurchaseId = payment.PurchaseId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status.ToString().ToLower(),
            CreatedAt = payment.CreatedAt,
            CompletedAt = payment.CompletedAt
        };
    }
}