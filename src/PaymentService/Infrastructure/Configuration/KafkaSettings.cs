namespace Infrastructure.Configuration;

public class KafkaSettings
{
    public string BootstrapServers { get; set; } = string.Empty;
    public Dictionary<string, string> TopicMap { get; set; } = new();
    public string Acks { get; set; } = "All";
    public bool EnableIdempotence { get; set; } = true;
    public int MaxInFlight { get; set; } = 5;
}
