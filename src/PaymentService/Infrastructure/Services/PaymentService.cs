using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Infrastructure.Data;
using Infrastructure.Extensions;
using Application.Interfaces.Gateways;
using Application.Interfaces.Services;
using Application.DTOs;
using Domain.Entities;
using Domain.Enums;

namespace Infrastructure.Services;

public class PaymentService: IPaymentService
{
    private readonly IPaymentGatewayFactory _paymentGatewayFactory;
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public PaymentService(
        IPaymentGatewayFactory paymentGatewayFactory, ILogger<PaymentService> logger,
        PaymentDbContext dbContext,
        ICurrentUserService currentUserService)
    {
        _paymentGatewayFactory = paymentGatewayFactory;
        _logger = logger;
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<PaymentSessionResponse> ProcessPaymentAsync(PaymentRequest paymentRequest, string idempotencyKey)
    {
        //Checking existing payment
        var existing = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.IdempotencyKey == idempotencyKey);
        if (existing != null)
        {
            _logger.LogInformation(
                "Found existing payment for idempotency key {Key}, retrieving session from provider",
                idempotencyKey);

            // Get gateway and retrieve the existing session using the same idempotency key
            var existingGateway = _paymentGatewayFactory.GetGateway(existing.ProviderId);
            var existingGatewayRequest = new GatewayChargeRequest
            {
                Amount = existing.Amount,
                Currency = existing.Currency,
                IdempotencyKey = idempotencyKey,
            };

            var existingGatewayResponse = await existingGateway.CreatePaymentSessionAsync(existingGatewayRequest);

            if (!existingGatewayResponse.Success)
            {
                _logger.LogError(
                    "Failed to retrieve existing session {SessionId}: {ErrorMessage}",
                    existing.ProviderPaymentId, existingGatewayResponse.ErrorMessage);
                throw new InvalidOperationException($"Failed to retrieve payment session: {existingGatewayResponse.ErrorMessage}");
            }

            return new PaymentSessionResponse
            {
                PaymentId = existing.Id,
                PaymentUrl = existingGatewayResponse.RedirectUrl ?? string.Empty,
                Status = existing.Status.ToString().ToLower()
            };
        }

        // Get gateway and create payment session first (outside of transaction)
        var gateway = _paymentGatewayFactory.GetGateway(paymentRequest.Provider);
        var gatewayRequest = new GatewayChargeRequest
        {
            Amount = paymentRequest.Amount,
            Currency = paymentRequest.Currency,
            IdempotencyKey = idempotencyKey,
        };

        var gatewayResponse = await gateway.CreatePaymentSessionAsync(gatewayRequest);

        if (!gatewayResponse.Success)
        {
            _logger.LogError("Payment session creation failed: {ErrorMessage}", gatewayResponse.ErrorMessage);
            throw new InvalidOperationException($"Payment session creation failed: {gatewayResponse.ErrorMessage}");
        }

        // Now create the database record with the session ID
        await using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                IdempotencyKey = idempotencyKey,
                UserId = _currentUserService.GetUserId().Value,
                PurchaseId = paymentRequest.ProductId,
                Amount = paymentRequest.Amount,
                Currency = paymentRequest.Currency,
                ProviderId = paymentRequest.Provider,
                ProviderPaymentId = gatewayResponse.ProviderPaymentId,
                Status = PaymentStatus.Processing,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _dbContext.Payments.AddAsync(payment);
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Created payment {PaymentId} with session {SessionId}",
                payment.Id, gatewayResponse.ProviderPaymentId);

            return new PaymentSessionResponse
            {
                PaymentId = payment.Id,
                PaymentUrl = gatewayResponse.RedirectUrl ?? string.Empty,
                Status = payment.Status.ToString().ToLower()
            };
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex, "Error saving payment record for session {SessionId}", gatewayResponse.ProviderPaymentId);
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
