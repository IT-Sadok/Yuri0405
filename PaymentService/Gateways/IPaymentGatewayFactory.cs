namespace PaymentService.Gateways;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(short providerId);
}