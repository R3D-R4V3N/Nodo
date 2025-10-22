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
    public static UserDto.CurrentUser ToCurrentUserDto(this ApplicationUser user, string email) =>
        new UserDto.CurrentUser
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl,
            Email = email,
            Biography = user.Biography,
            BirthDay = user.BirthDay,
            CreatedAt = user.CreatedAt,
            Interests = user.Interests
                .Select(i => new UserDto.Interest
                {
                    Type = i.Type,
                    Like = i.Like,
                    Dislike = i.Dislike,
                })
                .ToList(),
            Hobbies = user.Hobbies
                .Select(h =>
                {
                    var descriptor = HobbyCatalog.GetDescriptor(h.Hobby);
                    return new UserDto.Hobby
                    {
                        Id = h.Hobby.ToString(),
                        Name = descriptor.Name,
                        Emoji = descriptor.Emoji,
                    };
                })
                .ToList(),
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
