using PaymentService.Gateways;

namespace PaymentService.Services;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(short providerId);
}