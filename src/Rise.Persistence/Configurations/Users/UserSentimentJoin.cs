using Rise.Domain.Users;
using Rise.Domain.Users.Sentiment;

namespace Rise.Persistence.Configurations.Users;
public class UserSentimentJoin
{
    public int UserId { get; set; }
    public ApplicationUser User { get; set; } = null!;

    public int SentimentId { get; set; }
    public UserSentiment Sentiment { get; set; } = null!;
}
