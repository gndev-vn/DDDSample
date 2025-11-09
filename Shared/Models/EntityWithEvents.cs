namespace Shared.Models;

public interface IEntityWithEvents
{
    IReadOnlyCollection<DomainEvent> DomainEvents { get; }
    void AddDomainEvent(DomainEvent e);
    void ClearDomainEvents();
}

public class EntityWithEvents : Entity, IEntityWithEvents
{
    private readonly List<DomainEvent> _domainEvents = [];
    
    public IReadOnlyCollection<DomainEvent> DomainEvents => _domainEvents;
    
    public void AddDomainEvent(DomainEvent e)
    {
        _domainEvents.Add(e);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}