namespace Muzonia.DbEf;

public static class NewId
{
    public static EntityId Create()
    {
        return EntityId.CreateVersion7();
    }
}
