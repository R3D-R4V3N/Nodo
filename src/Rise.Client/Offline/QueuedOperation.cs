namespace Rise.Client.Offline;

public record QueuedOperation
{
    public int Id { get; init; }
    public string BaseAddress { get; init; } = string.Empty;
    public string Path { get; init; } = string.Empty;
    public string Method { get; init; } = "GET";
    public string? Body { get; init; }
    public string? ContentType { get; init; }
    public Dictionary<string, string>? Headers { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public Guid? ClientMessageId { get; init; }
    public int? ChatId { get; init; }
}
