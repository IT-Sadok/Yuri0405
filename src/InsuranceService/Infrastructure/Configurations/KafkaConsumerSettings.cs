namespace Infrastructure.Configurations;

public class KafkaConsumerSettings
{
    public const string SectionName = "KafkaConsumer";

    public string BootstrapServers { get; set; } = string.Empty;
    public string GroupId { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public bool EnableAutoCommit { get; set; } = false;
    public string AutoOffsetReset { get; set; } = "earliest";
}
