namespace PaymentService.Gateways;

public class PaymentGatewayFactory: IPaymentGatewayFactory
{
    private readonly IServiceProvider _serviceProvider;

    public PaymentGatewayFactory(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public IPaymentGateway GetGateway(short providerId)
    {
        return providerId switch
        {
            1 => _serviceProvider.GetRequiredService<StripeGateway>(),
            3 => _serviceProvider.GetRequiredService<MockGateway>(),
            _ => throw new ArgumentException($"Unknown provider: {providerId}")
        };
    }
}