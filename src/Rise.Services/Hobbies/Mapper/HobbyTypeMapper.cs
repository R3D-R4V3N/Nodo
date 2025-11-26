using Rise.Domain.Users.Hobbys;
using Rise.Shared.Hobbies;

namespace Rise.Services.Hobbies.Mapper;

internal static class HobbyTypeMapper
{
    public static HobbyTypeDto ToDto(this HobbyType category)
     => (HobbyTypeDto)category;

    public static HobbyType ToDomain(this HobbyTypeDto dto)
        => (HobbyType)dto;
}
