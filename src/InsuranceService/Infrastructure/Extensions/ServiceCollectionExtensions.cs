using Application.Interfaces;
using Application.Mediator;
using Infrastructure.BackgroundServices;
using Infrastructure.Configurations;
using Infrastructure.Data;
using Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMediator();

        // OrderService for ActivateOrderAsync (used by Kafka consumer)
        services.AddScoped<IOrderService, OrderService>();

        // Add other services
        services.AddScoped<IPaymentService, PaymentService>();

        // Add Kafka consumer
        services.Configure<KafkaConsumerSettings>(
            configuration.GetSection(KafkaConsumerSettings.SectionName));
        services.AddHostedService<PaymentEventConsumerBackgroundService>();

        return services;
    }

    public static IServiceCollection AddInsuranceDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InsuranceDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

        return services;
    }

    private static IServiceCollection AddMediator(this IServiceCollection services)
    {
        services.AddScoped<IMediator, Application.Mediator.Mediator>();

        var assembly = typeof(IRequestHandler<,>).Assembly;
        var handlerAssembly = typeof(Infrastructure.Services.Handlers.Commands.CreatePolicyCommandHandler).Assembly;

        var handlerTypes = handlerAssembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>))
                .Select(i => new { Interface = i, Implementation = t }));

        foreach (var handler in handlerTypes)
        {
            services.AddScoped(handler.Interface, handler.Implementation);
        }

        return services;
    }
}
