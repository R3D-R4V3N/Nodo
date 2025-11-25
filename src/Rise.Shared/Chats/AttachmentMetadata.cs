namespace Rise.Shared.Chats;

public record AttachmentMetadata
{
    public string? FileName { get; init; }
    public string? ContentType { get; init; }
    public string? BlobKey { get; init; }
}

public record MessageAttachment
{
    public string? FileName { get; init; }
    public string? ContentType { get; init; }
    public string? Url { get; init; }
    public string? BlobKey { get; init; }
}
