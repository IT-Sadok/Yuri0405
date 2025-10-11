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
        if (providerId == 3)
            return _serviceProvider.GetRequiredService<MockGateway>();
        throw new ArgumentException($"Unknown provider: {providerId}");
    }
}