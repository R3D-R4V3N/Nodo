using System.Collections.Generic;
using Rise.Domain.Common;

namespace Rise.Domain.Chats;

public abstract class ChatUser : Entity
{
    public ICollection<Chat> Chats { get; set; } = new List<Chat>();
}
