namespace Rise.Domain.Chats;

public class Chat : Entity
{
    private List<Message> _messages;

    public List<Message> Messages
    {
        get => _messages;
        set => _messages = value ?? throw new ArgumentNullException(nameof(value));
    }
}