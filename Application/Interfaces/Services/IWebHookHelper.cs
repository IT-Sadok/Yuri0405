using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IWebHookHelper
{
    Task HandleCheckoutSessionCompleted(WebhookSessionModel? session);
    Task HandleCheckoutSessionExpired(WebhookSessionModel? session);
    Task HandlePaymentIntentFailed(WebhookPaymentIntentModel? paymentIntent);
}
