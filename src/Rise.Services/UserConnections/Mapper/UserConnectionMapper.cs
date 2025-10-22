using Rise.Domain.Users;
<<<<<<< HEAD
using Rise.Shared.UserConnections;

namespace Rise.Services.UserConnections.Mapper;
public static class UserConnectionTypeMapper
{
    public static UserConnectionTypeDto MapToDto(this UserConnectionType type)
        => type switch
        {
            UserConnectionType.Friend => UserConnectionTypeDto.Friend,
            UserConnectionType.RequestIncoming => UserConnectionTypeDto.IncomingFriendRequest,
            UserConnectionType.RequestOutgoing => UserConnectionTypeDto.OutgoingFriendRequest,
            UserConnectionType.Blocked => UserConnectionTypeDto.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    public static UserConnectionType MapToDomain(this UserConnectionTypeDto type)
        => type switch
        {
            UserConnectionTypeDto.Friend => UserConnectionType.Friend,
            UserConnectionTypeDto.IncomingFriendRequest => UserConnectionType.RequestIncoming,
            UserConnectionTypeDto.OutgoingFriendRequest => UserConnectionType.RequestOutgoing,
            UserConnectionTypeDto.Blocked => UserConnectionType.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
=======
using Rise.Services.UserConnections.Mapper;
using Rise.Services.Users.Mapper;
using Rise.Shared.UserConnections;

namespace Rise.Services.UserConnections.Mapper;
public static class UserConnectionMapper
{
    public static UserConnectionDto.GetFriends ToIndexUserConnectionDto(this UserConnection user) =>
        new UserConnectionDto.GetFriends
        {
            User = user.Connection.ToConnectionDto(),
            State = user.ConnectionType.MapToDto(),
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
        };
}
