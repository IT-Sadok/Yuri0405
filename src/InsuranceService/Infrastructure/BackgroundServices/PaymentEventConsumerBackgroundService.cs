using System.Text.Json;
using Application.DTOs.Events;
using Application.Interfaces;
using Confluent.Kafka;
using Infrastructure.Configurations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.BackgroundServices;

public class PaymentEventConsumerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PaymentEventConsumerBackgroundService> _logger;
    private readonly KafkaConsumerSettings _settings;
    private readonly IConsumer<string, string> _consumer;

    public PaymentEventConsumerBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<PaymentEventConsumerBackgroundService> logger,
        IOptions<KafkaConsumerSettings> settings)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _settings = settings.Value;

        var config = new ConsumerConfig
        {
            BootstrapServers = _settings.BootstrapServers,
            GroupId = _settings.GroupId,
            EnableAutoCommit = _settings.EnableAutoCommit,
            AutoOffsetReset = _settings.AutoOffsetReset
        };

        _consumer = new ConsumerBuilder<string, string>(config).Build();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Yield to allow the web host to start before blocking on Kafka
        await Task.Yield();

        _logger.LogInformation("Payment event consumer starting. Subscribing to topic: {Topic}", _settings.Topic);

        _consumer.Subscribe(_settings.Topic);

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await ConsumeMessageAsync(stoppingToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Payment event consumer is stopping due to cancellation");
        }
        finally
        {
            _consumer.Close();
            _consumer.Dispose();
            _logger.LogInformation("Payment event consumer stopped");
        }
    }

    private async Task ConsumeMessageAsync(CancellationToken stoppingToken)
    {
        try
        {
            var consumeResult = _consumer.Consume(TimeSpan.FromSeconds(1));

            if (consumeResult == null)
            {
                return;
            }

            _logger.LogInformation(
                "Received message from topic {Topic}, partition {Partition}, offset {Offset}",
                consumeResult.Topic,
                consumeResult.Partition.Value,
                consumeResult.Offset.Value);

            await ProcessMessageAsync(consumeResult.Message.Value, stoppingToken);

            _consumer.Commit(consumeResult);

            _logger.LogInformation(
                "Committed offset {Offset} for partition {Partition}",
                consumeResult.Offset.Value,
                consumeResult.Partition.Value);
        }
        catch (ConsumeException ex)
        {
            _logger.LogError(ex, "Error consuming message from Kafka");
        }
    }

    private async Task ProcessMessageAsync(string messageValue, CancellationToken stoppingToken)
    {
        PaymentCompletedEvent? paymentEvent;

        try
        {
            paymentEvent = JsonSerializer.Deserialize<PaymentCompletedEvent>(messageValue, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to deserialize payment event. Message: {Message}", messageValue);
            return;
        }

        if (paymentEvent == null)
        {
            _logger.LogWarning("Deserialized payment event is null. Message: {Message}", messageValue);
            return;
        }

        _logger.LogInformation(
            "Processing PaymentCompletedEvent: EventId={EventId}, PaymentId={PaymentId}, PurchaseId={PurchaseId}, Amount={Amount} {Currency}",
            paymentEvent.Id,
            paymentEvent.PaymentId,
            paymentEvent.PurchaseId,
            paymentEvent.Amount,
            paymentEvent.Currency);

        using var scope = _scopeFactory.CreateScope();
        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        var result = await orderService.ActivateOrderAsync(
            paymentEvent.PurchaseId,
            paymentEvent.PaymentId.ToString());

        switch (result)
        {
            case OrderActivationResult.Success:
                _logger.LogInformation(
                    "Order {OrderId} activated successfully with payment reference {PaymentId}",
                    paymentEvent.PurchaseId,
                    paymentEvent.PaymentId);
                break;

            case OrderActivationResult.OrderNotFound:
                _logger.LogWarning(
                    "Order {OrderId} not found for payment {PaymentId}",
                    paymentEvent.PurchaseId,
                    paymentEvent.PaymentId);
                break;

            case OrderActivationResult.AlreadyProcessed:
                _logger.LogInformation(
                    "Order {OrderId} was already processed (idempotency check). Payment: {PaymentId}",
                    paymentEvent.PurchaseId,
                    paymentEvent.PaymentId);
                break;

            case OrderActivationResult.InvalidStatus:
                _logger.LogWarning(
                    "Order {OrderId} has invalid status for activation. Payment: {PaymentId}",
                    paymentEvent.PurchaseId,
                    paymentEvent.PaymentId);
                break;
        }
    }
}
