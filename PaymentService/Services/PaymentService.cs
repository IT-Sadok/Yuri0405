using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Extentions;
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
            return existing.ToResponse();
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
            ProviderId = paymentRequest.Provider,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
        
        await _dbContext.Payments.AddAsync(payment);
        await _dbContext.SaveChangesAsync();
        
        _logger.LogInformation(
            "Created payment {PaymentId} with status {Status}",
            payment.Id, payment.Status.ToString());

        //Update status to processing and call gateway within a transaction
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            payment.Status = PaymentStatus.Processing;
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var gateway = _paymentGatewayFactory.GetGateway(paymentRequest.Provider);
            var gatewayRequest = new GatewayChargeRequest
            {
                Amount = payment.Amount,
                Currency = payment.Currency,
                IdempotencyKey = idempotencyKey,
                PaymentToken = paymentRequest.PaymentToken
            };
            GatewayResponse result;
            try
            {
                result = await gateway.ChargeAsync(gatewayRequest);
            }
            catch (HttpRequestException e)
            {
                _logger.LogError(e,
                    "Network error occurred while processing payment {PaymentId}. This is a transient error that may succeed on retry.",
                    payment.Id);

                result = new GatewayResponse
                {
                    Success = false,
                    ProviderPaymentId = null,
                    ErrorMessage = "Network error: Gateway communication failure. Please retry."
                };
            }

            payment.ProviderPaymentId = result.ProviderPaymentId;
            payment.UpdatedAt = DateTime.UtcNow;

            if (result.Success)
            {
                payment.Status = PaymentStatus.Completed;
                payment.CompletedAt = DateTime.UtcNow;
                _logger.LogInformation(
                    "Payment {PaymentId} completed successfully",
                    payment.Id);
            }
            else
            {
                payment.Status = PaymentStatus.Failed;
                payment.FailureReason = result.ErrorMessage;

                // Handle specific decline reasons
                if (result.ErrorMessage?.Contains("Insufficient funds", StringComparison.OrdinalIgnoreCase) == true)
                {
                    _logger.LogWarning(
                        "Payment {PaymentId} declined due to insufficient funds for user {UserId}. Amount: {Amount} {Currency}",
                        payment.Id, payment.UserId, payment.Amount, payment.Currency);
                }
            }

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return payment.ToResponse();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            await _dbContext.Entry(payment).ReloadAsync();
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = $"System error: {ex.Message}";
            payment.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
            throw;
        }
    }

    public async Task<PaymentResponse> GetPaymentAsync(Guid paymentId)
    {
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.Id == paymentId);
        if (payment == null)
            throw new KeyNotFoundException($"Payment {paymentId} not found");
        return payment.ToResponse();
    }

    public async Task<IEnumerable<PaymentResponse>> GetAllPaymentsAsync()
    {
        var payments = await _dbContext.Payments
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(p => p.ToResponse());
    }
}