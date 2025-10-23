using Rise.Domain.Users;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;

internal static class HobbyMapper
{
    public static UserHobbyDto ToDto(UserHobby hobby)
    {
        var descriptor = HobbyCatalog.GetDescriptor(hobby.Hobby);

        return new UserHobbyDto
        {
            Id = hobby.Hobby.ToString(),
            Name = descriptor.Name,
            Emoji = descriptor.Emoji,
        };
    }
}
