using Rise.Domain.Chats;
using Rise.Domain.Users;

namespace Rise.Domain.Message;

public interface IMessage
{
    DateTime Timestamp { get; set; }
    IChat Chat { get; set; }
    
    // oh ye, super visor chat gulp
    IChatUser SendBy { get; set; }
}
