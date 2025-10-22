using Rise.Domain.Users;
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
        };
}
