namespace Application.DTOs;

public class OutboxPaymentMessage
{
    public Guid Id { get; set; }
    public DateTime OccuredOn { get; set; }
    public string Payload { get; set; }
    public bool IsProcessed { get; set; }
}
