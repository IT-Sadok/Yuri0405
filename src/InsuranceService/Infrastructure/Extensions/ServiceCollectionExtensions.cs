using Infrastructure.BackgroundServices;
using Infrastructure.Configurations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddKafkaConsumer(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<KafkaConsumerSettings>(
            configuration.GetSection(KafkaConsumerSettings.SectionName));

        services.AddHostedService<PaymentEventConsumerBackgroundService>();

        return services;
    }
}
