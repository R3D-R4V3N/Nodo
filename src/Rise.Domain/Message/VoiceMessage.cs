using Rise.Domain.Chats;
using Rise.Domain.Users;

namespace Rise.Domain.Message;

public class VoiceMessage : Entity, IMessage
{
    public DateTime Timestamp { get; set; }

    // no clue how it actually gets stored
    public required object Blob { get; set; }
    public required string Encoding { get; set; }
    public int Length { get; set; }
    public required IChat Chat { get; set; }
    public required IChatUser SendBy { get; set; }
}
