using PaymentService.Models.DTOs;
using Stripe;
namespace PaymentService.Gateways;

public class StripeGateway: IPaymentGateway
{
    private readonly ChargeService _chargeService;
    
    public string ProviderName => "stripe";

    public StripeGateway(IConfiguration configuration)
    {
        _chargeService = new ChargeService();
        StripeConfiguration.ApiKey = configuration["Gateways:Stripe:SecretKey"];
    }

    public async Task<GatewayResponse> ChargeAsync(GatewayChargeRequest request)
    {
        try
        {
            var options = new ChargeCreateOptions
            {
                Amount = (long)(request.Amount*100), //Stripe uses cents
                Currency = request.Currency.ToLower(),
                Source = "tok_visa",
                Description = $"Payment via {ProviderName}",
            };
            var charge = await _chargeService.CreateAsync(options);
            return new GatewayResponse
            {
                Success = charge.Status == "succeeded",
                ProviderPaymentId = charge.Id,
                Status = charge.Status,
                ErrorMessage = charge.FailureMessage
            };
        }
        catch (StripeException ex)
        {
            // Card declined, invalid request, etc.
            return new GatewayResponse
            {
                Success = false,
                ErrorMessage = ex.Message,
                Status = "failed"
            };
        }
        catch (Exception ex)
        {
            // Network errors - let them bubble up for retry logic
            throw new HttpRequestException("Stripe connection failed", ex);
        }
    }
    
    public Task<GatewayResponse> RefundAsync(string providerPaymentId, decimal amount)
    {
        throw new NotImplementedException();
    }
}