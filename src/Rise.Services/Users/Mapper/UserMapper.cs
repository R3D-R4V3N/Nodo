using System;
using System.Collections.Generic;
using System.Linq;
using Rise.Domain.Users;
using Rise.Services.Hobbies.Mapper;
using Rise.Services.Sentiments.Mapper;
using Rise.Shared.Hobbies;
using Rise.Shared.Sentiments;
using Rise.Shared.Users;

namespace Rise.Services.Users.Mapper;

internal static class UserMapper
{
    public static UserDto.CurrentUser ToCurrentUserDto(
        this BaseUser user,
        string email,
        IEnumerable<SentimentDto.Get>? sentiments = null,
        IEnumerable<HobbyDto.Get>? hobbies = null,
        IEnumerable<string>? defaultChatLines = null)
        => new UserDto.CurrentUser
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl,
            Email = email,
            Biography = user.Biography,
            Gender = user.Gender.ToDto(),
            BirthDay = user.BirthDay,
            CreatedAt = user.CreatedAt,
            Interests = sentiments?.ToList() ?? [],
            Hobbies = hobbies?.ToList() ?? [],
            DefaultChatLines = defaultChatLines?.ToList()
                ?? user.UserSettings.ChatTextLineSuggestions
                    .Select(x => x.Sentence.Value)
                    .ToList(),
        };

    public static UserDto.Connection ToConnectionDto(this User user) =>
        new UserDto.Connection
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            Age = CalculateAge(user.BirthDay),
            AvatarUrl = user.AvatarUrl,
        };
    public static UserDto.Connection ToConnectionDto(this User user, int chatId) =>
        new UserDto.Connection
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            Age = CalculateAge(user.BirthDay),
            AvatarUrl = user.AvatarUrl,
            ChatId = chatId == default ? string.Empty : chatId.ToString(),
        };
    public static UserDto.CurrentUser ToCurrentUserDto(this User user, string email) =>
        ((BaseUser)user).ToCurrentUserDto(
            email,
            user.Sentiments.Select(SentimentMapper.ToGetDto),
            user.Hobbies.Select(HobbyMapper.ToGetDto),
            user.UserSettings.ChatTextLineSuggestions.Select(x => x.Sentence.Value));
    public static UserDto.ConnectionProfile ToConnectionProfileDto(this User user) =>
        new UserDto.ConnectionProfile
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl,
           
            Biography = user.Biography,
            Gender = user.Gender.ToDto(),
            BirthDay = user.BirthDay,
            
            Hobbies = user.Hobbies
                .Select(HobbyMapper.ToGetDto)
                .ToList(),
        };

    public static UserDto.Chat ToChatDto(this BaseUser user) =>
        new UserDto.Chat
        {
            Id = user.Id,
            Name = $"{user.FirstName} {user.LastName}",
            AccountId = user.AccountId,
            AvatarUrl = user.AvatarUrl
        };

    public static UserDto.Message ToMessageDto(this BaseUser user) =>
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
