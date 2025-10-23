namespace Rise.Shared.Users;

public record UserInterestDto
{
    public string Type { get; init; } = string.Empty;
    public string? Like { get; init; }
    public string? Dislike { get; init; }
}
