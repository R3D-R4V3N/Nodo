using Rise.Domain.Users.Hobbys;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;

internal static class HobbyTypeMapper
{
    public static HobbyTypeDto ToDto(this HobbyType category)
     => (HobbyTypeDto)category;

    public static HobbyType ToDomain(this HobbyTypeDto dto)
        => (HobbyType)dto;
}
