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
            Age = CalculateAge(user.BirthDay),
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
                .Select(x => x.Sentence.Value)
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

    private static int CalculateAge(DateOnly birthDay)
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var age = today.Year - birthDay.Year;

        if (today < birthDay.AddYears(age))
        {
            age--;
        }

        return age;
    }
}
