using PaymentService.Models.Enums;

namespace PaymentService.Gateways;

public interface IPaymentGatewayFactory
{
    IPaymentGateway GetGateway(PaymentProvider provider);
}