using Rise.Domain.Users;
using Rise.Domain.Users.Sentiment;

namespace Rise.Persistence.Configurations.Users.Sentiments;
public class UserSentimentJoin
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public int SentimentId { get; set; }
    public UserSentiment Sentiment { get; set; } = null!;
}
