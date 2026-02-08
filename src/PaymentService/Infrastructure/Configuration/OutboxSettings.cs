namespace Infrastructure.Configuration;

public class OutboxSettings
{
    public int ProcessingIntervalSeconds { get; set; } = 5;
    public int BatchSize { get; set; } = 20;
}
