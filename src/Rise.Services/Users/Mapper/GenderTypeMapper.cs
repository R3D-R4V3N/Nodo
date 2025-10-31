using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Properties;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;

internal static class GenderTypeMapper
{
    public static GenderTypeDto ToDto(this GenderType gender)
     => (GenderTypeDto)gender;

    public static GenderType ToDomain(this GenderTypeDto dto)
        => (GenderType)dto;
}
