namespace Rise.Client.Offline;

public class CacheObject<T>
{
    public required string Key { get; set; }
    public required T Payload { get; set; }
}
