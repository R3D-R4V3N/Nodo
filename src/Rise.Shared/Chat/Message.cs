namespace Rise.Shared.Chat;

public sealed record Message(
    string Id,
    string Text,
    bool IsOutgoing,
    string? AvatarUrl = null,
    DateTimeOffset? Timestamp = null
);