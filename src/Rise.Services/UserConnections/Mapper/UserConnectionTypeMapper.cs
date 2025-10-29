using Rise.Domain.Users.Connections;
using Rise.Shared.UserConnections;

namespace Rise.Services.UserConnections.Mapper;
public static class UserConnectionTypeMapper
{
    public static UserConnectionTypeDto MapToDto(this UserConnectionType type)
        => type switch
        {
            UserConnectionType.None => UserConnectionTypeDto.None,
            UserConnectionType.Friend => UserConnectionTypeDto.Friend,
            UserConnectionType.RequestIncoming => UserConnectionTypeDto.IncomingFriendRequest,
            UserConnectionType.RequestOutgoing => UserConnectionTypeDto.OutgoingFriendRequest,
            UserConnectionType.Blocked => UserConnectionTypeDto.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    public static UserConnectionType MapToDomain(this UserConnectionTypeDto type)
        => type switch
        {
            UserConnectionTypeDto.None => UserConnectionType.None,
            UserConnectionTypeDto.Friend => UserConnectionType.Friend,
            UserConnectionTypeDto.IncomingFriendRequest => UserConnectionType.RequestIncoming,
            UserConnectionTypeDto.OutgoingFriendRequest => UserConnectionType.RequestOutgoing,
            UserConnectionTypeDto.Blocked => UserConnectionType.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
}
