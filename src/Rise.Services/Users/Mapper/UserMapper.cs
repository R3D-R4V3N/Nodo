using System;
using System.Linq;
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

    public static UserDto.CurrentUser ToCurrentUserDto(this ApplicationUser user, string email = "") =>
        new UserDto.CurrentUser
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl,
            Email = email,

            Biography = user.Biography,
            Gender = user.Gender,
            BirthDay = user.BirthDay,
            CreatedAt = user.CreatedAt,
            Interests = user.Sentiments
                .Select(SentimentMapper.ToGetDto)
                .ToList(),
            Hobbies = user.Hobbies
                .Select(HobbyMapper.ToGetDto)
                .ToList(),
            DefaultChatLines = user
                .UserSettings
                .ChatTextLineSuggestions
                .Select(x => x.Text)
                .ToList(),
        };
    public static UserDto.ConnectionProfile ToConnectionProfileDto(this ApplicationUser user) =>
        new UserDto.ConnectionProfile
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl,
           
            Biography = user.Biography,
            Gender = user.Gender,
            BirthDay = user.BirthDay,
            
            Hobbies = user.Hobbies
                .Select(HobbyMapper.ToGetDto)
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
