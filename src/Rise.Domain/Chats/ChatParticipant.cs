using Ardalis.GuardClauses;
using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class ChatParticipant : Entity
{
    public int ChatId { get; private set; }

    public Chat Chat { get; private set; } = null!;

    public int UserId { get; private set; }

    public ApplicationUser User { get; private set; } = null!;

    private ChatParticipant()
    {
    }

    public ChatParticipant(Chat chat, ApplicationUser user)
    {
        Chat = Guard.Against.Null(chat);
        User = Guard.Against.Null(user);
        ChatId = chat.Id;
        UserId = user.Id;
    }
}
