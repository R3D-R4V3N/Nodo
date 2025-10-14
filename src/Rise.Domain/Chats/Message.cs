using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class Message : Entity
{
    public string? Inhoud { get; set; }

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public int SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;

    public string? AudioContentType { get; set; }
    public byte[]? AudioData { get; set; }
    public double? AudioDurationSeconds { get; set; }
}
