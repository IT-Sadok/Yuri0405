using Microsoft.AspNetCore.Mvc;
using PaymentService.Services;
using Stripe;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class WebhookController : ControllerBase
{
    private readonly ILogger<WebhookController> _logger;
    private readonly IWebHookHelper _webHookHelper;
    private readonly IConfiguration _configuration;

    public WebhookController(
        ILogger<WebhookController> logger,
        IWebHookHelper webHookHelper,
        IConfiguration configuration)
    {
        _logger = logger;
        _webHookHelper = webHookHelper;
        _configuration = configuration;
    }

    [HttpPost("stripe")]
    public async Task<IActionResult> StripeWebhook()
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

        try
        {
            var stripeEvent = EventUtility.ParseEvent(json);
            var webhookSecret = _configuration["Gateways:Stripe:WebhookSecret"];

            _logger.LogInformation("Received Stripe webhook event: {EventType}", stripeEvent.Type);

            // Verify webhook signature if secret is configured
            if (!string.IsNullOrEmpty(webhookSecret))
            {
                var signatureHeader = Request.Headers["Stripe-Signature"];
                stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, webhookSecret);
            }

            // Handle the event
            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                    var completedSession = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    await _webHookHelper.HandleCheckoutSessionCompleted(completedSession);
                    break;

                case EventTypes.CheckoutSessionExpired:
                    var expiredSession = stripeEvent.Data.Object as Stripe.Checkout.Session;
                    await _webHookHelper.HandleCheckoutSessionExpired(expiredSession);
                    break;

                case EventTypes.PaymentIntentPaymentFailed:
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    await _webHookHelper.HandlePaymentIntentFailed(paymentIntent);
                    break;

                default:
                    _logger.LogInformation(
                        "Received unhandled Stripe event type: {EventType}. Event ID: {EventId}",
                        stripeEvent.Type,
                        stripeEvent.Id);
                    break;
            }

            return Ok();
        }
        catch (StripeException ex)
        {
            _logger.LogError(ex, "Stripe webhook error");
            return BadRequest();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Webhook processing error");
            return StatusCode(500);
        }
    }
}
