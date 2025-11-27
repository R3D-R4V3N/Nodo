using Rise.Domain.Messages;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;
public class MessageHistoryItem : Entity
{
    public required Chat Chat { get; set; }
    public required BaseUser User { get; set; }
    public required Message LastReadMessage { get; set; }
}
