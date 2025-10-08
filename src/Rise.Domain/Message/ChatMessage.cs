using System;
using Rise.Domain.Chats;
using Rise.Domain.Common;

namespace Rise.Domain.Message;

public abstract class ChatMessage : Entity
{
    public DateTime Timestamp { get; set; }

    public virtual Chat Chat { get; set; } = default!;

    public virtual ChatUser SendBy { get; set; } = default!;
}
