using Rise.Domain.Chats;
using Rise.Domain.Messages.Properties;
using Rise.Domain.Users;

namespace Rise.Domain.Messages;

public class Message : Entity
{
    public Chat Chat { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
    public Text? Text { get; set; }
    public string? AudioContentType { get; set; }
    public byte[]? AudioData { get; set; }
    public double? AudioDurationSeconds { get; set; }
}