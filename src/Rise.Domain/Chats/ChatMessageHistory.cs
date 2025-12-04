using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class ChatMessageHistory : Entity
{
    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime? LastReadAt { get; set; }
    public int? LastReadMessageId { get; set; }
}
