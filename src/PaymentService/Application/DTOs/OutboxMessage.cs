namespace Application.DTOs;

public class OutboxMessage
{
    public Guid Id { get; set; }
    public DateTime OccuredOn { get; set; }
    public string Type { get; set; }
    public string Payload { get; set; }
    public DateTime? ProcessedOn { get; set; }
}
