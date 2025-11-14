using PaymentService.Models.DTOs;
using Stripe;
using Stripe.Checkout;
namespace PaymentService.Gateways;

public class StripeGateway: IPaymentGateway
{
    private readonly SessionService _sessionService;

    public string ProviderName => "stripe";

    public StripeGateway(IConfiguration configuration)
    {
        _sessionService = new SessionService();
        StripeConfiguration.ApiKey = configuration["Gateways:Stripe:SecretKey"];
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
                SuccessUrl = "https://example.com/success",
                CancelUrl = "https://example.com/cancel",
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