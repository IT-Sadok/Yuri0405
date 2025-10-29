using Microsoft.EntityFrameworkCore;
using PaymentService.Data;
using PaymentService.Gateways;
using PaymentService.Helpers;
using PaymentService.Services;

namespace PaymentService.Extentions;

public static class ServiceCollectionExtentions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddControllers();
        services.AddExceptionHandler<PaymentExceptionHandler>();
        services.AddProblemDetails();
        services.AddScoped<MockGateway>();
        services.AddScoped<StripeGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IPaymentService, Services.PaymentService>();
        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddPaymentDatabase(this IServiceCollection services)
    {
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseInMemoryDatabase("PaymentsDb"));

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
