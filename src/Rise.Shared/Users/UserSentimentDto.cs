namespace Rise.Shared.Users;

public record UserSentimentDto
{
    public SentimentTypeDto Type { get; init; }
    public string? Text { get; init; }
    public string? Emoji { get; init; }
}
