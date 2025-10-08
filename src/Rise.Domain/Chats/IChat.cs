using Rise.Domain.Message;

namespace Rise.Domain.Chats;

public interface IChat
{
    List<IChatUser> Users { get; set; }
    IMessage? LatestMessage { get => Messages.FirstOrDefault(); }
    List<IMessage> Messages { get; set; }
}
