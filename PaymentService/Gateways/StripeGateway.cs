using Microsoft.Extensions.Options;
using PaymentService.Models.Configurations;
using Application.DTOs;
using Application.Interfaces.Gateways;
using Domain.Enums;
using Stripe;
using Stripe.Checkout;
namespace PaymentService.Gateways;

public class StripeGateway: IPaymentGateway
{
    private readonly SessionService _sessionService;
    private readonly StripeSettings _stripeSettings;

    public PaymentProvider ProviderName => PaymentProvider.Stripe;

    public StripeGateway(IOptions<StripeSettings> stripeSettings)
    {
        _sessionService = new SessionService();
        _stripeSettings = stripeSettings.Value;
        StripeConfiguration.ApiKey = _stripeSettings.SecretKey;
    }

    public Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount)
    {
        throw new NotImplementedException();
    }

    public async Task<GatewayResponse> CreatePaymentSessionAsync(GatewayChargeRequest request)
    {
        try
        {
            var options = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = request.Currency.ToString().ToLower(),
                            UnitAmount = (long)(request.Amount * 100), // Stripe uses cents
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = $"Payment via {ProviderName}",
                            },
                        },
                        Quantity = 1,
                    },
                },
                Mode = "payment",
                SuccessUrl = _stripeSettings.SuccessUrl,
                CancelUrl = _stripeSettings.CancelUrl,
            };

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = request.IdempotencyKey
            };

            var session = await _sessionService.CreateAsync(options, requestOptions);

            return new GatewayResponse
            {
                Success = true,
                ProviderPaymentId = session.Id,
                RedirectUrl = session.Url,
            };
        }
        catch (StripeException ex)
        {
            return new GatewayResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
            };
        }
        catch (Exception ex)
        {
            throw new HttpRequestException("Stripe connection failed", ex);
        }
    }
}