namespace Muzonia.DbEf.Entities;

public class Entity
{
    public EntityId Id { get; set; } = NewId.Create();
    public DateTime CreationDate { get; set; } = DateTime.UtcNow;
}
