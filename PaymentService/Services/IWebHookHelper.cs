using Stripe;
using Stripe.Checkout;

namespace PaymentService.Services;

public interface IWebHookHelper
{
    Task HandleCheckoutSessionCompleted(Session? session);
    Task HandleCheckoutSessionExpired(Session? session);
    Task HandlePaymentIntentFailed(PaymentIntent? paymentIntent);
}
