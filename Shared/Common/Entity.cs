namespace Shared.Common;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();
    
    public DateTime UpdatedAtUtc { get; set; }
    
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public override bool Equals(object? obj)
        => obj is Entity other
           && other.GetType() == GetType()
           && Id.Equals(other.Id);

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    protected Entity()
    {
    }
}