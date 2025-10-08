using System.Collections.Generic;
using System.Linq;
using Rise.Domain.Common;
using Rise.Domain.Message;

namespace Rise.Domain.Chats;

public abstract class Chat : Entity
{
    public virtual List<ChatUser> Users { get; set; } = [];

    public virtual List<ChatMessage> Messages { get; set; } = [];

    public ChatMessage? LatestMessage => Messages.FirstOrDefault();
}
