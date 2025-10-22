using Ardalis.Result;
using Rise.Domain.Users;

namespace Rise.Domain.Chats;

public class Chat : Entity
{
    private List<ApplicationUser> _users = [];
    public IReadOnlyList<ApplicationUser> Users => _users.AsReadOnly();

    private List<Message> _messages = [];
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();
    public Chat()
    {
    }

    // TODO: maybe AddUser to CreateChat that accepts 2 users
    // reason is so you can check if they have a connection with eachother
    //public Result CreateChat(ApplicationUser user1, ApplicationUser user2)
    //{
    //    if (!user1.HasFriend(user2) || !user2.HasFriend(user1))
    //    {
    //        return Result.Conflict($"Chat kan niet worden gemaakt omdat {user1} en {user2} elkaar niet bevriend zijn");
    //    }

    //    _users = [user1, user2];

    //    user1.AddChat(this);
    //    user2.AddChat(this);

    //    return Result.Success();
    //}
    public Result AddUser(ApplicationUser user)
    {
        if (_users.Contains(user))
        { 
            return Result.Conflict($"Chat bevat deze gebruiker al {user}");
        }

        _users.Add(user);
        user.AddChat(this);

        return Result.Success();
    }
    public Result RemoveUser(ApplicationUser user)
    {
        if (!_users.Contains(user))
        {
            return Result.Conflict($"Chat bevat deze gebruiker niet {user}");
        }

        _users.Remove(user);
        user.RemoveChat(this);

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
            Text = text,
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