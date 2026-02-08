using Domain.Enums;

namespace Application.Interfaces.Gateways;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(PaymentProvider provider);
}
