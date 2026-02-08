namespace Domain.Events;

public abstract class BaseDomainEvent
{
    public Guid Id { get; set; }
    public DateTime OccuredOn { get; set; }
}
