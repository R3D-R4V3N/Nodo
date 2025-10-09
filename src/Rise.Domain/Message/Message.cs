using System;
using Rise.Domain.Chats;
using Rise.Domain.Common;

namespace Rise.Domain.Message;

public abstract class Message : Entity
{
    public DateTime Timestamp { get; set; }
    public required Chat Chat { get; set; }
    public required ChatUser SendBy { get; set; }
}
