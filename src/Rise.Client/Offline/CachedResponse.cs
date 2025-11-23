namespace Rise.Client.Offline;

public record CachedResponse
{
    public string Key { get; init; } = string.Empty;
    public int Status { get; init; }
    public string? Body { get; init; }
    public string? ContentType { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
}
