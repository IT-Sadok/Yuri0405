using System.Text.Json.Serialization;
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
        services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter());
        });
        services.AddExceptionHandler<PaymentExceptionHandler>();
        services.AddProblemDetails();
        services.AddScoped<MockGateway>();
        services.AddScoped<StripeGateway>();
        services.AddScoped<IPaymentGatewayFactory, PaymentGatewayFactory>();
        services.AddScoped<IPaymentService, Services.PaymentService>();
        services.AddScoped<IWebHookHelper, WebHookStripeHelper>();
        services.AddAuthorization();

        return services;
    }

    public static IServiceCollection AddPaymentDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        // In-memory database (for testing)
        // services.AddDbContext<PaymentDbContext>(options =>
        //     options.UseInMemoryDatabase("PaymentsDb"));

        // PostgreSQL database
        services.AddDbContext<PaymentDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        return services;
    }
}
