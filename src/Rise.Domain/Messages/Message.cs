using Rise.Domain.Chats;
using Rise.Domain.Users;

namespace Rise.Domain.Messages;

public class Message : Entity
{
    public Chat Chat { get; set; } = null!;
    public BaseUser Sender { get; set; } = null!;
    public TextMessage? Text { get; set; }
    public string? AudioDataUrl { get; set; }
    public double? AudioDurationSeconds { get; set; }
}