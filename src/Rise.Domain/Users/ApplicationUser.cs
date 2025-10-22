<<<<<<< HEAD
using Ardalis.GuardClauses;
using Ardalis.Result;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Rise.Domain.Common;
=======
using Ardalis.Result;
using Rise.Domain.Chats;
>>>>>>> codex/add-alert-message-for-supervisor-monitoring

namespace Rise.Domain.Users;

public class ApplicationUser : Entity
{
    /// <summary>
    /// Link to the <see cref="IdentityUser"/> account so chatprofielen gekoppeld blijven aan hun login.
    /// </summary>
    public string AccountId { get; private set; }
<<<<<<< HEAD

        private string _firstName = string.Empty;
        public required string FirstName
        {
            get => _firstName;
            set => _firstName = Guard.Against.NullOrWhiteSpace(value);
        }
        private string _lastName = string.Empty;
        public required string LastName
        {
            get => _lastName;
            set => _lastName = Guard.Against.NullOrWhiteSpace(value);
        }
        private string _biography = string.Empty;
        public required string Biography
        {
            get => _biography;
            set => _biography = Guard.Against.NullOrWhiteSpace(value);
        }
        public required DateOnly BirthDay { get; set; }
        public required UserType UserType { get; set; }
    

        //// connections
        private readonly HashSet<UserConnection> _connections = [];
        public IReadOnlyCollection<UserConnection> Connections => _connections;
        public IEnumerable<UserConnection> Friends => _connections
            .Where(x => x.ConnectionType.Equals(UserConnectionType.Friend));
        public IEnumerable<UserConnection> FriendRequests => _connections
            .Where(x => 
                x.ConnectionType.Equals(UserConnectionType.RequestIncoming) 
                || x.ConnectionType.Equals(UserConnectionType.RequestOutgoing));
        public IEnumerable<UserConnection> BlockedUsers => _connections
            .Where(x => x.ConnectionType.Equals(UserConnectionType.Blocked));

        public ApplicationUser()
=======

    private string _firstName = string.Empty;
    public required string FirstName
    {
        get => _firstName;
        set => _firstName = Guard.Against.NullOrWhiteSpace(value);
    }
    private string _lastName = string.Empty;
    public required string LastName
    {
        get => _lastName;
        set => _lastName = Guard.Against.NullOrWhiteSpace(value);
    }
    private string _biography = string.Empty;
    public required string Biography
    {
        get => _biography;
        set => _biography = Guard.Against.NullOrWhiteSpace(value);
    }
    private string _avatarUrl = string.Empty;
    public required string AvatarUrl
    {
        get => _avatarUrl;
        set => _avatarUrl = Guard.Against.NullOrWhiteSpace(value);
    }
    public required DateOnly BirthDay { get; set; }
    public required UserType UserType { get; set; }
    

    //// connections
    private readonly HashSet<UserConnection> _connections = [];
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
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
        {
            if (_userSettings == value) return;

<<<<<<< HEAD
        public ApplicationUser(string accountId)
        {
            AccountId = Guard.Against.NullOrEmpty(accountId);
        }

        public Result<string> AddFriend(ApplicationUser friend)
        {
            bool isAdded = Friends
                .Any(x => x.Connection.Equals(friend));

            if (isAdded)
                return Result.Conflict($"User is already friends with {friend}");

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

                return Result.Success($"User send a friend request to {friend}");
            }

            if (friendRequest.ConnectionType.Equals(UserConnectionType.RequestOutgoing))
            {
                return Result.Conflict($"User has already send a request to {friend}");
            }

            _connections.Add(new 
                UserConnection() 
                { 
                    Connection = friend, 
                    ConnectionType = UserConnectionType.Friend 
                }
            );

            _connections.Remove(
                new UserConnection()
                { 
                    Connection = friend, 
                    ConnectionType = UserConnectionType.RequestIncoming 
                }
            );

            friend._connections.Add(new
                UserConnection()
                {
                    Connection = this,
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

            return Result.Success($"User added {friend}");
        }

        public Result RemoveFriend(ApplicationUser friend)
        {
            _connections.Remove(
                new UserConnection()
                {
                    Connection = friend,
                    ConnectionType = UserConnectionType.Friend,
                }
            );

            friend._connections.Remove(
                new UserConnection()
                {
                    Connection = this,
                    ConnectionType = UserConnectionType.Friend
                }
            );

            return Result.Success();
        }
    }

=======
            _userSettings = Guard.Against.Null(value);
            if (_userSettings.User != this)
            {
                _userSettings.User = this;
            }
        }
    }

    public ApplicationUser()
    {
    }

    public ApplicationUser(string accountId)
    {
        AccountId = Guard.Against.NullOrEmpty(accountId);
    }

    public bool HasFriend(ApplicationUser friend) 
        => _connections.Contains(new UserConnection() { Connection = friend, ConnectionType = UserConnectionType.Friend});

    public Result<string> AddFriend(ApplicationUser friend)
    {
        bool isAdded = Friends
            .Any(x => x.Connection.Equals(friend));

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

        _connections.Add(new 
            UserConnection() 
            { 
                Connection = friend, 
                ConnectionType = UserConnectionType.Friend 
            }
        );

        _connections.Remove(
            new UserConnection()
            { 
                Connection = friend, 
                ConnectionType = UserConnectionType.RequestIncoming 
            }
        );

        friend._connections.Add(new
            UserConnection()
            {
                Connection = this,
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

        return Result.Success($"Gebruiker voegt {friend} toe");
    }

    public Result RemoveFriend(ApplicationUser friend)
    {
        _connections.Remove(
            new UserConnection()
            {
                Connection = friend,
                ConnectionType = UserConnectionType.Friend,
            }
        );

        friend._connections.Remove(
            new UserConnection()
            {
                Connection = this,
                ConnectionType = UserConnectionType.Friend
            }
        );

        return Result.Success();
    }

    public Result AddChat(Chat chat)
    {
        if (_chats.Contains(chat))
        {
            return Result.Conflict($"Gebruiker is al lid van chat {chat}");
        }

        _chats.Add(chat);
        chat.AddUser(this);

        return Result.Success();
    }
    public Result RemoveChat(Chat chat)
    {
        if (!_chats.Contains(chat))
        {
            return Result.Conflict($"Gebruiker is geen lid van chat {chat}");
        }

        _chats.Remove(chat);
        chat.RemoveUser(this);

        return Result.Success();
    }
}

>>>>>>> codex/add-alert-message-for-supervisor-monitoring
