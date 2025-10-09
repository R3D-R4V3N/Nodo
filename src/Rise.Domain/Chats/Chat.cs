using System.Collections.Generic;
using Rise.Domain.Common;
using Rise.Domain.Message;

namespace Rise.Domain.Chats;

public abstract class Chat : Entity
{
    public ICollection<ChatUser> Users { get; set; } = new List<ChatUser>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
}
