using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Confluent.Kafka;
using Infrastructure.Configuration;
using Infrastructure.Data;

namespace Infrastructure.BackgroundServices;

public class OutboxProcessorBackgroundService : BackgroundService
{
    private readonly ILogger<OutboxProcessorBackgroundService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly KafkaSettings _kafkaSettings;
    private readonly IProducer<string, string> _producer;

    public OutboxProcessorBackgroundService(
        ILogger<OutboxProcessorBackgroundService> logger,
        IServiceProvider serviceProvider,
        IOptions<KafkaSettings> kafkaSettings)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _kafkaSettings = kafkaSettings.Value;

        var config = new ProducerConfig
        {
            BootstrapServers = _kafkaSettings.BootstrapServers,
            Acks = Acks.All,
            EnableIdempotence = true,
            MaxInFlight = 5,
            MessageSendMaxRetries = 3
        };

        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Outbox Processor Background Service is starting");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessOutboxMessages(stoppingToken);
                await Task.Delay(
                    TimeSpan.FromSeconds(_kafkaSettings.ProcessingIntervalSeconds),
                    stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while processing outbox messages");
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }
        }

        _logger.LogInformation("Outbox Processor Background Service is stopping");
    }

    private async Task ProcessOutboxMessages(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();

        var unprocessedMessages = await dbContext.OutboxMessages
            .Where(m => m.ProcessedOn == null)
            .OrderBy(m => m.OccuredOn)
            .Take(_kafkaSettings.BatchSize)
            .ToListAsync(cancellationToken);

        if (unprocessedMessages.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Processing {Count} outbox messages",
            unprocessedMessages.Count);

        foreach (var message in unprocessedMessages)
        {
            try
            {
                var kafkaMessage = new Message<string, string>
                {
                    Key = message.Id.ToString(),
                    Value = message.Payload
                };

                var deliveryResult = await _producer.ProduceAsync(
                    _kafkaSettings.PaymentCompletedTopic,
                    kafkaMessage,
                    cancellationToken);

                _logger.LogInformation(
                    "Message {MessageId} published to Kafka topic {Topic} at offset {Offset}",
                    message.Id,
                    deliveryResult.Topic,
                    deliveryResult.Offset);

                message.ProcessedOn = DateTime.UtcNow;
                await dbContext.SaveChangesAsync(cancellationToken);

                _logger.LogInformation(
                    "Message {MessageId} marked as processed",
                    message.Id);
            }
            catch (ProduceException<string, string> ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to publish message {MessageId} to Kafka. Error: {Error}",
                    message.Id,
                    ex.Error.Reason);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Unexpected error while processing message {MessageId}",
                    message.Id);
            }
        }
    }

    public override void Dispose()
    {
        _producer?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
