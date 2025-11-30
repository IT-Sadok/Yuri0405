using Application.DTOs;

namespace Application.Interfaces.Services;

public interface IWebHookHelper
{
    Task HandleCheckoutSessionCompleted(WebhookSessionDto? session);
    Task HandleCheckoutSessionExpired(WebhookSessionDto? session);
    Task HandlePaymentIntentFailed(WebhookPaymentIntentDto? paymentIntent);
}
