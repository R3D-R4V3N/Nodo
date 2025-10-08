using Rise.Domain.Chats;
using Rise.Domain.Users;

namespace Rise.Domain.Message;

public class TextMessage : Entity, IMessage
{
    public DateTime Timestamp { get; set; }
    public string Text { get; set; }
    public required IChat Chat { get; set; }
    public required IChatUser SendBy { get; set; }
}
