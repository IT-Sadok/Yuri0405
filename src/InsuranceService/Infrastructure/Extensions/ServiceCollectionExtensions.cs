using Application.Interfaces;
using Infrastructure.BackgroundServices;
using Infrastructure.Configurations;
using Infrastructure.Data;
using Infrastructure.Services;
using Infrastructure.Services.Commands;
using Infrastructure.Services.Queries;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Add command services
        services.AddScoped<IPolicyCommandService, PolicyCommandService>();
        services.AddScoped<IOrderCommandService, OrderCommandService>();

        // Add query services
        services.AddScoped<IPolicyQueryService, PolicyQueryService>();
        services.AddScoped<IOrderQueryService, OrderQueryService>();

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
}
