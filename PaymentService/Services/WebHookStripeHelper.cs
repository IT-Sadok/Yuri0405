using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Models.Enums;
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

    public async Task HandleCheckoutSessionCompleted(Session? session)
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

        payment.Status = PaymentStatus.Completed;
        payment.CompletedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Payment {PaymentId} marked as completed for session {SessionId}",
            payment.Id, session.Id);
    }

    public async Task HandleCheckoutSessionExpired(Session? session)
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

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = "Checkout session expired";
        payment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Payment {PaymentId} marked as failed (expired) for session {SessionId}",
            payment.Id, session.Id);
    }

    public async Task HandlePaymentIntentFailed(PaymentIntent? paymentIntent)
    {
        if (paymentIntent == null) return;

        var payment = await _dbContext.Payments
            .FirstOrDefaultAsync(p => p.ProviderPaymentId == paymentIntent.Id);

        if (payment == null)
        {
            _logger.LogWarning("Payment not found for PaymentIntent {PaymentIntentId}", paymentIntent.Id);
            return;
        }

        if (payment.Status == PaymentStatus.Completed || payment.Status == PaymentStatus.Failed)
        {
            _logger.LogInformation("Payment {PaymentId} already in terminal state", payment.Id);
            return;
        }

        payment.Status = PaymentStatus.Failed;
        payment.FailureReason = paymentIntent.LastPaymentError?.Message ?? "Payment failed";
        payment.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "Payment {PaymentId} marked as failed for PaymentIntent {PaymentIntentId}. Reason: {Reason}",
            payment.Id, paymentIntent.Id, payment.FailureReason);
    }
}
