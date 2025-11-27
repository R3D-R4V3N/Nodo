using Ardalis.Result;
using Rise.Domain.Users;
using Rise.Domain.Messages;
using Rise.Domain.Common.ValueObjects;

namespace Rise.Domain.Chats;
public class Chat : Entity
{
    // ef
    private Chat() { }

    private readonly List<BaseUser> _users = [];
    public IReadOnlyList<BaseUser> Users => _users.AsReadOnly();
    
    private readonly List<Message> _messages = [];
    public IReadOnlyList<Message> Messages => _messages.AsReadOnly();
    public ChatType ChatType { get; private set; }

    public static Result<Chat> CreatePrivateChat(BaseUser baseUser1, BaseUser baseUser2)
    {
        if (baseUser1 is User user1 && baseUser2 is User user2)
        {
            if (!user1.IsFriend(user2) || !user2.IsFriend(user1))
            {
                return Result.Conflict(
                    $"Chat kan niet worden gemaakt omdat {user1} en {user2} elkaar niet bevriend zijn"
                );
            }
        }

        Chat chat = new Chat()
        {
            ChatType = ChatType.Private,
        };

        chat._users.Add(baseUser1);
        chat._users.Add(baseUser2);

        baseUser1.AddChat(baseUser2, chat);
        baseUser2.AddChat(baseUser1, chat);

        return Result.Success(chat);
    }

    public static Result<Chat> CreateGroupChat(BaseUser chatOwner, params BaseUser[] users)
    {
        foreach (var user in users)
        {
            if (chatOwner is User user1 && user is User user2)
            {
                if (!user1.IsFriend(user2) || !user2.IsFriend(user1))
                {
                    return Result.Conflict(
                        $"Chat kan niet worden gemaakt omdat {user1} en {user2} elkaar niet bevriend zijn"
                    );
                }
            }
        }

        Chat chat = new Chat()
        { 
            ChatType = ChatType.Group,
        };

        chat._users.Add(chatOwner);
        chat._users.AddRange(users);

        var temp = users.First();

        chatOwner.AddChat(temp, chat);
        foreach (var user in users)
        {
            user.AddChat(chatOwner, chat);
        }

        return Result.Success(chat);
    }

    public Result AddUser(BaseUser chatOwner, BaseUser user)
    {
        if (chatOwner is not Supervisor && user is not Supervisor && ChatType.Equals(ChatType.Private))
        { 
            return Result.Conflict($"Er kunnen geen gebruikers toegevoegd worden aan een private chat");
        }

        if (chatOwner is not Supervisor && !_users.Contains(chatOwner))
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

    public Result RemoveUser(BaseUser chatOwner, BaseUser user)
    {
        if (chatOwner is not Supervisor && ChatType.Equals(ChatType.Private))
        {
            return Result.Conflict($"Er kunnen geen gebruikers verwijderd worden van een private chat");
        }

        if (chatOwner is not Supervisor && !_users.Contains(chatOwner))
        {
            return Result.Conflict($"Meegegeven gebruiker: {chatOwner} is geen chat eigenaar");
        }
        if (!_users.Contains(user))
        {
            return Result.Conflict($"Chat bevat {user} niet");
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

    public Result AddTextMessage(string text, BaseUser user)
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
            Text = TextMessage.Create(text),
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