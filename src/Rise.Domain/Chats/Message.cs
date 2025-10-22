using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class Message : Entity
{
    public Chat Chat { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
    public string? Text { get; set; }
    public string? AudioContentType { get; set; }
    public byte[]? AudioData { get; set; }
    public double? AudioDurationSeconds { get; set; }
}
