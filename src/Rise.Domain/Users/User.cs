using Ardalis.Result;
using Rise.Domain.Chats;
using Rise.Domain.Helper;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Settings;

namespace Rise.Domain.Users;

public class User : BaseUser
{
    //// connections
    private readonly HashSet<UserConnection> _connections = [];
    public IReadOnlyCollection<UserConnection> Connections => _connections;
    public IEnumerable<UserConnection> Friends => _connections
        .Where(x => x.ConnectionType is UserConnectionType.Friend);
    public IEnumerable<UserConnection> FriendRequests => _connections
        .Where(x => x.ConnectionType is UserConnectionType.RequestIncoming or UserConnectionType.RequestOutgoing);
    public IEnumerable<UserConnection> BlockedUsers => _connections
        .Where(x => x.ConnectionType is UserConnectionType.Blocked);

    public bool IsFriend(User req) 
        => (GetConnection(req)?.ConnectionType ?? UserConnectionType.None) == UserConnectionType.Friend;
    public bool IsBlocked(User req)
        => (GetConnection(req)?.ConnectionType ?? UserConnectionType.None) == UserConnectionType.Blocked;
    public bool HasFriendRequest(User req)
    { 
        var type = GetConnection(req)?.ConnectionType ?? UserConnectionType.None;
        return type.Equals(UserConnectionType.RequestIncoming) || type.Equals(UserConnectionType.RequestOutgoing);
    }

    private UserConnection? GetConnection(User target)
    {
        foreach (var conType in Enum.GetValues<UserConnectionType>())
        {
            var con = _connections.FirstOrDefault(c =>
                c.From.Id == this.Id &&
                c.To.Id == target.Id &&
                c.ConnectionType == conType
            );

            if (con is not null)
                return con;

            //var key = this.CreateConnectionWith(target, conType);
            //if (_connections.TryGetValue(key, out var con))
            //{
            //    return con;
            //}
        }

        return null;
    }

    public Result<User> SendFriendRequest(User target)
    {
        UserConnection? targetConn = target.GetConnection(this);

        if (targetConn is not null && targetConn.ConnectionType.Equals(UserConnectionType.Blocked))
        {
            return Result.Conflict($"{target} heeft je geblokkeerd");
        }

        UserConnection? conn = GetConnection(target);

        if (conn is not null)
        {
            string conflictMessage = conn.ConnectionType switch
            {
                UserConnectionType.Friend
                    => $"{this} is al bevriend met {target}",
                UserConnectionType.RequestIncoming 
                    => $"{target} heeft al een verzoek verstuurd, accepteer hem!",
                UserConnectionType.RequestOutgoing
                    => $"{this} heeft al een vriendschapsverzoek naar {target} verstuurd",
                UserConnectionType.Blocked
                    => $"Gebruiker {target} is geblokkeerd",
                _ => throw new NotImplementedException(conn.ConnectionType.ToString()),
            };

            return Result.Conflict(conflictMessage);
        }

        _connections.Add(
            this.CreateConnectionWith(target, UserConnectionType.RequestOutgoing)
        );

        target._connections.Add(
            target.CreateConnectionWith(this, UserConnectionType.RequestIncoming)
        );

        return Result.Success(this, $"{this} verstuurd een vriendschapsverzoek naar {target}");
    }

    public Result<User> AcceptFriendRequest(User target)
    {
        UserConnection? conn = GetConnection(target);

        if (conn is null)
        {
            return Result.Conflict($"Er is geen veroek van {target} om te accepteren");
        }

        if (conn.ConnectionType.Equals(UserConnectionType.Friend))
        {
            return Result.Conflict($"{this} is al bevriend met {target}");
        }

        if (!conn.ConnectionType.Equals(UserConnectionType.RequestIncoming))
        { 
            return Result.Conflict($"Er is geen verzoek van {target} om te accepteren");
        }

        Span<User> span =
        [
            this, target
        ];

        for (int i = 0; i < span.Length; i++)
        {
            var current = span[i];
            var opposite = span[(i + 1) % span.Length];
            current._connections.Remove(
                current.CreateConnectionWith(opposite, UserConnectionType.RequestIncoming)
            );
            current._connections.Remove(
                current.CreateConnectionWith(opposite, UserConnectionType.RequestOutgoing)
            );
            current._connections.Add(
                current.CreateConnectionWith(opposite, UserConnectionType.Friend)
            );
        }

        return Result.Success(this, $"{this} is nu bevriend met {target}");
    }

    public Result<User> RemoveFriendRequest(User friend)
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
                current.CreateConnectionWith(opposite, UserConnectionType.RequestIncoming)
            );
            current._connections.Remove(
                current.CreateConnectionWith(opposite, UserConnectionType.RequestOutgoing)
            );
        }

        return Result.Success(this, $"vriendschap verzoek verwijderd van {friend}");
    }

    public Result<User> RemoveFriend(User friend)
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
                current.CreateConnectionWith(opposite, UserConnectionType.Friend)
            );
        }

        return Result.Success(this, $"vriendschap beeindigd met {friend}");
    }

    public override string ToString()
    {
        return $"{FirstName} {LastName}";
    }
}

