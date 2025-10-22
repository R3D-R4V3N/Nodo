using Rise.Domain.Users;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;
internal static class UserMapper
{
    public static UserDto.Connection ToConnectionDto(this ApplicationUser user) =>
        new UserDto.Connection
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            Age = DateTime.Now.Year - user.BirthDay.Year -
                    (DateTime.Now.DayOfYear < user.BirthDay.DayOfYear ? 1 : 0),
            AvatarUrl = user.AvatarUrl,
        };
    public static UserDto.CurrentUser ToCurrentUserDto(this ApplicationUser user) =>
        new UserDto.CurrentUser
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl,
            DefaultChatLines = user
                .UserSettings
                .ChatTextLineSuggestions
                .Select(x => x.Text)
                .ToList(),
        };

    public static UserDto.Chat ToChatDto(this ApplicationUser user) =>
        new UserDto.Chat
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl
        };

    public static UserDto.Message ToMessageDto(this ApplicationUser user) =>
        new UserDto.Message
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl
        };
}
