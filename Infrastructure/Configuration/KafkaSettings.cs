namespace Infrastructure.Configuration;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public string PaymentCompletedTopic { get; set; } = "payment-completed";
    public int BatchSize { get; set; } = 10;
    public int ProcessingIntervalSeconds { get; set; } = 5;
}
