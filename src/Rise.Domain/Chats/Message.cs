using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class Message : Entity
{
<<<<<<< HEAD
    public string? Inhoud { get; set; }

    public int ChatId { get; set; }
=======
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
    public Chat Chat { get; set; } = null!;
    public ApplicationUser Sender { get; set; } = null!;
<<<<<<< HEAD

=======
    public string? Text { get; set; }
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
    public string? AudioContentType { get; set; }
    public byte[]? AudioData { get; set; }
    public double? AudioDurationSeconds { get; set; }
}
