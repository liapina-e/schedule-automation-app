namespace schedule_automation_app_server.Domain.Common;

public abstract class BaseEntity
{
    public Guid Id { get; protected set; }
    
    protected BaseEntity() => Id = Guid.NewGuid();
    
    protected BaseEntity(Guid id) => Id = id;
    
    public override bool Equals(object obj)
    {
        if (GetType() != obj.GetType())
        {
            return false;
        }
        
        var other = (BaseEntity)obj;
        return Id == other.Id;
    }
    
    public override int GetHashCode() => Id.GetHashCode();
    
    public static bool operator ==(BaseEntity left, BaseEntity right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
        {
            return true;
        }

        if (ReferenceEquals(left, null) || ReferenceEquals(right, null))
        {
            return false;
        }
            
        return left.Equals(right);
    }
    
    public static bool operator !=(BaseEntity left, BaseEntity right)
    {
        return !(left == right);
    }
}