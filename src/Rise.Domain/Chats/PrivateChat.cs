using Rise.Domain.Message;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class PrivateChat : Entity, IChat
{
    public List<IChatUser> Users { get; set; } = [];
    public List<IMessage> Messages { get; set; } = [];
}
