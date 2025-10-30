using Ardalis.Result;
using Rise.Domain.Users;
using Rise.Domain.Messages;
using Rise.Domain.Messages.Properties;

namespace Rise.Domain.Chats;

public class Chat : Entity
{
    // ef
    private Chat() { }

    private readonly List<ApplicationUser> _users = [];
    public IReadOnlyList<ApplicationUser> Users => _users.AsReadOnly();
    
    private readonly List<Message> _messages = [];
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();

    public static Result<Chat> CreateChat(ApplicationUser user1, ApplicationUser user2)
    {
        if (!user1.HasFriend(user2) || !user2.HasFriend(user1))
        {
            return Result.Conflict($"Chat kan niet worden gemaakt omdat {user1} en {user2} elkaar niet bevriend zijn");
        }
        Chat chat = new Chat();

        chat._users.Add(user1);
        chat._users.Add(user2);

        user1.AddChat(user2, chat);
        user2.AddChat(user1, chat);

        return Result.Success(chat);
    }

    public Result AddUser(ApplicationUser chatOwner, ApplicationUser user)
    {
        if (!chatOwner.IsSupervisor() && !_users.Contains(chatOwner))
        {
            return Result.Conflict($"Meegegeven gebruiker: {chatOwner} is geen chat eigenaar");
        }
        if (_users.Contains(user))
        {
            return Result.Conflict($"Chat bevat deze gebruiker al {user}");
        }

        _users.Add(user);
        if (!user.Chats.Contains(this))
        { 
            var res = user.AddChat(chatOwner, this);
            if (!res.IsSuccess)
            {
                _users.Remove(user);
                return res;
            }
        }

        return Result.Success();
    }

    public Result RemoveUser(ApplicationUser chatOwner, ApplicationUser user)
    {
        if (!chatOwner.IsSupervisor() && !_users.Contains(chatOwner))
        {
            return Result.Conflict($"Meegegeven gebruiker: {chatOwner} is geen chat eigenaar");
        }
        if (!_users.Contains(user))
        {
            return Result.Conflict($"Chat bevat deze gebruiker niet {user}");
        }

        _users.Remove(user);
        if (user.Chats.Contains(this))
        {
            var res = user.RemoveChat(chatOwner, this);
            if (!res.IsSuccess)
            {
                _users.Add(user);
                return res;
            }
        }

        return Result.Success();
    }

    public Result AddTextMessage(string text, ApplicationUser user)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return Result.Conflict($"Bericht is leeg");
        }

        if (!_users.Contains(user))
        {
            return Result.Conflict($"Gebruiker behoort niet tot deze chat {this}");
        }

        var message = new Message()
        {
            Text = Text.Create(text),
            Sender = user,
            Chat = this
        };

        _messages.Add(message);

        return Result.Success();
    }

    public Result AddMessage(Message message)
    {
        message.Chat ??= this;

        if (!message.Chat.Equals(this))
        {
            return Result.Conflict($"Bericht behoort niet tot deze chat {this}");
        }

        if (!_users.Contains(message.Sender))
        {
            return Result.Conflict($"Gebruiker behoort niet tot deze chat {this}");
        }

        _messages.Add(message);

        return Result.Success();
    }

    public Result RemoveMessage(Message message)
    {
        _messages.Remove(message);
        return Result.Success();
    }
}