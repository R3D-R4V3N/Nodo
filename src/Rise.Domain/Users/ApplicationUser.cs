using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Organizations;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;

namespace Rise.Domain.Users;

public class ApplicationUser : Entity
{
    // ef
    public ApplicationUser() { }

    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account so chatprofielen gekoppeld blijven aan hun login.
    /// </summary>
    public string AccountId { get; private set; }
    public required FirstName FirstName { get; set; }
    public required LastName LastName { get; set; }
    public required Biography Biography { get; set; }
    public required AvatarUrl AvatarUrl { get; set; }
    public required DateOnly BirthDay { get; set; }
    public required UserType UserType { get; set; }
    public int OrganizationId { get; private set; }

    private Organization _organization = default!;
    public required Organization Organization
    {
        get => _organization;
        set
        {
            _organization = Guard.Against.Null(value);
            OrganizationId = value.Id;
        }
    }
    

    //// connections
    private readonly List<UserConnection> _connections = new();
    public IReadOnlyCollection<UserConnection> Connections => _connections;
    public IEnumerable<UserConnection> Friends => _connections
        .Where(x => x.ConnectionType.Equals(UserConnectionType.Friend));
    public IEnumerable<UserConnection> FriendRequests => _connections
        .Where(x => 
            x.ConnectionType.Equals(UserConnectionType.RequestIncoming) 
            || x.ConnectionType.Equals(UserConnectionType.RequestOutgoing));
    public IEnumerable<UserConnection> BlockedUsers => _connections
        .Where(x => x.ConnectionType.Equals(UserConnectionType.Blocked));

    // chats
    private readonly List<Chat> _chats = [];
    public IReadOnlyList<Chat> Chats => _chats.AsReadOnly();

    // settings
    private ApplicationUserSetting _userSettings;
    public required ApplicationUserSetting UserSettings
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

    public ApplicationUser(string accountId)
    {
        AccountId = Guard.Against.NullOrEmpty(accountId);
    }
    public bool IsSupervisor()
        => this.UserType.Equals(UserType.Supervisor);

    public bool HasFriend(ApplicationUser friend) 
        => _connections.Contains(new UserConnection() { Connection = friend, ConnectionType = UserConnectionType.Friend});

    public Result<string> RejectFriendRequest(ApplicationUser requester)
    {
        UserConnection? friendRequest = FriendRequests
            .FirstOrDefault(x => x.Connection.Equals(requester));

        if (friendRequest is null)
        {
            return Result.NotFound($"Er is geen vriendschapsverzoek van {requester} gevonden");
        }

        _connections.Remove(
            new UserConnection()
            {
                Connection = requester,
                ConnectionType = UserConnectionType.RequestIncoming
            }
        );

        requester._connections.Remove(
            new UserConnection()
            {
                Connection = this,
                ConnectionType = UserConnectionType.RequestOutgoing
            }
        );

        return Result.Success($"Gebruiker weigert vriendschapsverzoek van {requester}");
    }
    
    public Result<string> AddFriend(ApplicationUser friend)
    {       
        bool isAdded = Friends.Any(x => x.Connection?.Equals(friend) ?? false);

        if (isAdded)
        {
            return Result.Conflict($"Gebruiker is al bevriend met {friend}");
        }
        
        UserConnection? friendRequest = FriendRequests
            .FirstOrDefault(x => x.Connection.Equals(friend));

        if (friendRequest is null)
        {
            _connections.Add(
                new UserConnection()
                {
                    Connection = friend,
                    ConnectionType = UserConnectionType.RequestOutgoing
                }
            );

            friend._connections.Add(
                new UserConnection() 
                { 
                    Connection = this,
                    ConnectionType = UserConnectionType.RequestIncoming 
                }
            );

            return Result.Success($"Gebruiker verstuurd een vriendschapsverzoek naar {friend}");
        }

        if (friendRequest.ConnectionType.Equals(UserConnectionType.RequestOutgoing))
        {
            return Result.Conflict($"Gebruiker heeft al een vriendschapsverzoek naar {friend} verstuurd");
        }
        
        _connections.Remove(
            new UserConnection()
            {
                Connection = friend,
                ConnectionType = UserConnectionType.RequestIncoming
            }
        );

        _connections.Add(
            new UserConnection()
            {
                Connection = friend,
                ConnectionType = UserConnectionType.Friend
            }
        );

        friend._connections.Remove(
            new UserConnection()
            {
                Connection = this,
                ConnectionType = UserConnectionType.RequestOutgoing
            }
        );

        friend._connections.Add(
            new UserConnection()
            {
                Connection = this,
                ConnectionType = UserConnectionType.Friend
            }
        );

        return Result.Success($"Gebruiker voegt {friend} toe");
    }

    public Result RemoveFriend(ApplicationUser friend)
    {
        Span<ApplicationUser> span =
        [
            this, friend
        ];

        for (int i = 0; i < span.Length; i++)
        {
            var current = span[i];
            var opposite = span[(i + 1) % span.Length];
            current._connections.Remove(
                new UserConnection()
                {
                    Connection = opposite,
                    ConnectionType = UserConnectionType.Friend,
                }
            );
        }

        return Result.Success();
    }

    public Result RemoveFriendRequest(ApplicationUser friend)
    {
        Span<ApplicationUser> span =
        [
            this, friend
        ];

        for (int i = 0; i < span.Length; i++)
        {
            var current = span[i];
            var opposite = span[(i + 1) % span.Length];
            current._connections.Remove(
                new UserConnection()
                {
                    Connection = opposite,
                    ConnectionType = UserConnectionType.RequestIncoming,
                }
            );
            current._connections.Remove(
                new UserConnection()
                {
                    Connection = opposite,
                    ConnectionType = UserConnectionType.RequestOutgoing,
                }
            );
        }

        return Result.Success();
    }

    public Result AddChat(ApplicationUser chatOwner, Chat chat)
    {
        if (_chats.Contains(chat))
        {
            return Result.Conflict($"Gebruiker is al lid van chat {chat}");
        }
        if (!this.IsSupervisor() && !chatOwner.HasFriend(this))
        {
            return Result.Conflict($"Chat eigenaar is niet bevriendt met {this}");
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
    public Result RemoveChat(ApplicationUser chatOwner, Chat chat)
    {
        if (!chatOwner.IsSupervisor() && !_chats.Contains(chat))
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

