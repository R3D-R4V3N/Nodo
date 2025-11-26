using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Users.Settings;

namespace Rise.Domain.Users;

public abstract class BaseUser : Entity
{
    // ef
    public BaseUser() { }

    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account so chatprofielen gekoppeld blijven aan hun login.
    /// </summary>
    public string AccountId { get; init; }
    public required FirstName FirstName { get; set; }
    public required LastName LastName { get; set; }
    public required Biography Biography { get; set; }
    public required AvatarUrl AvatarUrl { get; set; }
    public required BirthDay BirthDay { get; set; }
    public required GenderType Gender { get; set; }    
    // settings
    private UserSetting _userSettings;
    public required UserSetting UserSettings
    {
        get => _userSettings;
        set
        {
            if (_userSettings == value) return;

            _userSettings = Guard.Against.Null(value);
            if (_userSettings.User != this)
            {
                _userSettings.User = this;
            }
        }
    }

    // chats
    private readonly List<Chat> _chats = [];
    public IReadOnlyList<Chat> Chats => _chats.AsReadOnly();

    public Result AddChat(BaseUser chatOwner, Chat chat)
    {
        if (_chats.Contains(chat))
        {
            return Result.Conflict($"Gebruiker is al lid van chat {chat}");
        }

        if (chatOwner is User chatOwnerUser && this is User currentUser)
        {
            if (!chatOwnerUser.IsFriend(currentUser))
            {
                return Result.Conflict(
                    $"{chatOwnerUser} is niet bevriendt met {currentUser}"
                );
            }
        }

        _chats.Add(chat);
        if (!chat.Users.Contains(this))
        {
            var res = chat.AddUser(chatOwner, this);
            if (!res.IsSuccess)
            {
                _chats.Remove(chat);
                return res;
            }
        }

        return Result.Success();
    }

    public Result RemoveChat(BaseUser chatOwner, Chat chat)
    {
        if (!_chats.Contains(chat))
        {
            return Result.Conflict($"Gebruiker is geen lid van chat {chat}");
        }

        _chats.Remove(chat);
        if (chat.Users.Contains(this))
        {
            var res = chat.RemoveUser(chatOwner, this);
            if (!res.IsSuccess)
            {
                _chats.Add(chat);
                return res;
            }
        }

        return Result.Success();
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}

