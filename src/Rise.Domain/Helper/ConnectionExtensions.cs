using Rise.Domain.Users;
using Rise.Domain.Users.Connections;

namespace Rise.Domain.Helper;
public static class ConnectionExtensions
{
    public static UserConnection CreateConnectionWith(this User user, User to, UserConnectionType type)
        => new UserConnection()
        {
            From = user,
            To = to,
            ConnectionType = type,
        };
}
