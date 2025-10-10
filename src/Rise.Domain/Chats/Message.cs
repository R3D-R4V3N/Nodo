using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class Message : Entity
{
    private string _inhoud = string.Empty;

    public string Inhoud
    {
        get => _inhoud;
        set => _inhoud = Guard.Against.NullOrWhiteSpace(value);
    }

    public int ChatId { get; set; }
    public Chat Chat { get; set; } = null!;

    public int SenderId { get; set; }
    public ApplicationUser Sender { get; set; } = null!;
}
