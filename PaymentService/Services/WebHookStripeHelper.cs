using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using Application.Interfaces.Services;
using Application.DTOs;
using Domain.Enums;
using Stripe;
using Stripe.Checkout;

namespace PaymentService.Services;

public class WebHookStripeHelper : IWebHookHelper
{
    private readonly ILogger<WebHookStripeHelper> _logger;
    private readonly PaymentDbContext _dbContext;

    public WebHookStripeHelper(
        ILogger<WebHookStripeHelper> logger,
        PaymentDbContext dbContext)
    {
        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task HandleCheckoutSessionCompleted(WebhookSessionDto? session)
    {
        if (session == null) return;

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == session.Id);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found for session {SessionId}", session.Id);
            return;
        }

        if (payment.Status == PaymentStatus.Completed)
        {
            _logger.LogInformation("Payment {PaymentId} already completed", payment.Id);
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            payment.Status = PaymentStatus.Completed;
            payment.FailureReason = null;
            payment.CompletedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Payment {PaymentId} marked as completed for session {SessionId}",
                payment.Id, session.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Error updating payment {PaymentId} for session {SessionId}",
                payment.Id, session.Id);
            throw;
        }
    }

    public async Task HandleCheckoutSessionExpired(WebhookSessionDto? session)
    {
        if (session == null) return;

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == session.Id);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found for session {SessionId}", session.Id);
            return;
        }

        if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Failed)
        {
            _logger.LogInformation("Payment {PaymentId} already in terminal state", payment.Id);
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = "Checkout session expired";
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Payment {PaymentId} marked as failed (expired) for session {SessionId}",
                payment.Id, session.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Error updating payment {PaymentId} for session {SessionId}",
                payment.Id, session.Id);
            throw;
        }
    }

    public async Task HandlePaymentIntentFailed(WebhookPaymentIntentDto? paymentIntent)
    {
        if (paymentIntent == null) return;

        // Retrieve the checkout session using the payment intent ID (outside transaction)
        var sessionService = new SessionService();
        var listOptions = new SessionListOptions
        {
            PaymentIntent = paymentIntent.Id,
            Limit = 1
        };

        var sessions = await sessionService.ListAsync(listOptions);
        var session = sessions.Data.FirstOrDefault();

        if (session == null)
        {
            _logger.LogWarning(
                "Checkout session not found for PaymentIntent {PaymentIntentId}",
                paymentIntent.Id);
            return;
        }

        // Find the local payment record using the checkout session ID
        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == session.Id);

        if (payment == null)
        {
            _logger.LogWarning(
                "Payment not found for session {SessionId} (PaymentIntent {PaymentIntentId})",
                session.Id, paymentIntent.Id);
            return;
        }

        if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Failed)
        {
            _logger.LogInformation("Payment {PaymentId} already in terminal state", payment.Id);
            return;
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        try
        {
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
            payment.UpdatedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();

            _logger.LogInformation(
                "Payment {PaymentId} marked as failed for session {SessionId} (PaymentIntent {PaymentIntentId}). Reason: {Reason}",
                payment.Id, session.Id, paymentIntent.Id, payment.FailureReason);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            _logger.LogError(ex,
                "Error updating payment {PaymentId} for session {SessionId} (PaymentIntent {PaymentIntentId})",
                payment.Id, session.Id, paymentIntent.Id);
            throw;
        }
    }
}
