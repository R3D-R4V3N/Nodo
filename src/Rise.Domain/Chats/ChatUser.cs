using System.Collections.Generic;
using Rise.Domain.Common;

namespace Rise.Domain.Chats;

public abstract class ChatUser : Entity
{
    public virtual List<Chat> Chats { get; set; } = [];
}
