using PaymentService.Models.Enums;

namespace PaymentService.Gateways;

public class PaymentGatewayFactory: IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IPaymentGateway GetGateway(PaymentProvider provider)
    {
        return provider switch
        {
            PaymentProvider.Stripe => _serviceProvider.GetRequiredService<StripeGateway>(),
            PaymentProvider.Mock => _serviceProvider.GetRequiredService<MockGateway>(),
            _ => throw new ArgumentException($"Unknown provider: {provider}")
        };
    }
}