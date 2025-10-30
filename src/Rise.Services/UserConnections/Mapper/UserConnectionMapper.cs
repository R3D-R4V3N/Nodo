﻿using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Services.Users.Mapper;
using Rise.Shared.UserConnections;

namespace Rise.Services.UserConnections.Mapper;
public static class UserConnectionMapper
{
    public static UserConnectionDto.Get ToGetDto(this UserConnection user) =>
        new UserConnectionDto.Get
        {
            User = user.Connection.ToConnectionDto(),
            State = user.ConnectionType.MapToDto(),
        };

    public static UserConnectionDto.Get ToGetDto(this ApplicationUser user) =>
        new UserConnectionDto.Get
        {
            User = user.ToConnectionDto(),
            State = UserConnectionTypeDto.None,
        };
    
}
