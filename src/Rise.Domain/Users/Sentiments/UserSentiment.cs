namespace Rise.Domain.Users.Sentiment;
public class UserSentiment : Entity
{
    public required SentimentType Type { get; set; }
    public required SentimentCategoryType Category { get; set; }

    public UserSentiment()
    {
    }
}
