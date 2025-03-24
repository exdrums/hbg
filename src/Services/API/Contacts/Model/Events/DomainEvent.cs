namespace API.Contacts.Model;

/// <summary>
/// Base class for domain events
/// </summary>
public abstract class DomainEvent
{
    public DateTime Timestamp { get; private set; }

    protected DomainEvent()
    {
        Timestamp = DateTime.UtcNow;
    }
}
