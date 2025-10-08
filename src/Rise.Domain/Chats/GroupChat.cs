using Rise.Domain.Message;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class GroupChat : Entity, IChat 
{
    public string GroupName { get; set; } = string.Empty;
    public List<IChatUser> Users { get; set; } = [];
    public List<IMessage> Messages { get; set; } = [];
}
