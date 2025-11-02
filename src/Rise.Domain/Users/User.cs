using Ardalis.GuardClauses;
using Ardalis.Result;
using System.Collections.Generic;
using Rise.Domain.Chats;
using Rise.Domain.Users.Connections;

namespace Rise.Domain.Users;

public class User : BaseUser
{
    private Supervisor _supervisor = default!;
    public required Supervisor Supervisor
    {
        get => _supervisor;
        set
        {
            if (_supervisor == value)
            {
                return;
            }

            _supervisor?.DetachUser(this);
            _supervisor = Guard.Against.Null(value);
            SupervisorId = _supervisor.Id;
            _supervisor.AttachUser(this);
        }
    }

    public int SupervisorId { get; private set; }

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

    public bool HasFriend(User friend) 
        => _connections.Contains(new UserConnection() { Connection = friend, ConnectionType = UserConnectionType.Friend});

    public Result<string> RejectFriendRequest(User requester)
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
    
    public Result<string> AddFriend(User friend)
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

    public Result RemoveFriend(User friend)
    {
        Span<User> span =
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

    public Result RemoveFriendRequest(User friend)
    {
        Span<User> span =
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

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}

