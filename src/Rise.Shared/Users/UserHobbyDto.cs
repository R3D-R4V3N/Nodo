namespace Rise.Shared.Users;

public record UserHobbyDto
{
    public string Id { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Emoji { get; init; } = string.Empty;
}
