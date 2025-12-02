using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Application.Interfaces.Services;
using Application.DTOs;
using Stripe;

namespace PaymentService.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
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
            var stripeObject = stripeEvent.Data.Object;

            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted when stripeObject is Stripe.Checkout.Session completedSession:
                    await _webHookHelper.HandleCheckoutSessionCompleted(new WebhookSessionModel { Id = completedSession.Id });
                    break;

                case EventTypes.CheckoutSessionExpired when stripeObject is Stripe.Checkout.Session expiredSession:
                    await _webHookHelper.HandleCheckoutSessionExpired(new WebhookSessionModel { Id = expiredSession.Id });
                    break;

                case EventTypes.PaymentIntentPaymentFailed when stripeObject is PaymentIntent paymentIntent:
                    await _webHookHelper.HandlePaymentIntentFailed(new WebhookPaymentIntentModel
                    {
                        Id = paymentIntent.Id,
                        LastPaymentError = paymentIntent.LastPaymentError != null
                            ? new PaymentErrorModel { Message = paymentIntent.LastPaymentError.Message }
                            : null
                    });
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
