using System;
using System.Collections.Generic;
using System.Linq;
using Rise.Shared.Sentiments;
using Rise.Shared.Users;

namespace Rise.Client.Profile.Models;

public record ProfileInterestModel(string Type, string? Like, string? Dislike);

public record ProfileHobbyModel(string Id, string Name, string Emoji);

public record HobbyOption(string Id, string Name, string Emoji);

public record PreferenceOption(string Id, string Name, string? Emoji = null)
{
    public string Label => string.IsNullOrWhiteSpace(Emoji) ? Name : $"{Emoji} {Name}";
}

public record PreferenceChip(string Id, string Label);

public record ProfileModel
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public GenderTypeDto Gender { get; init; }
    public DateOnly BirthDay { get; init; }
    public string AvatarUrl { get; init; } = DefaultAvatar;
    public string MemberSince { get; init; } = string.Empty;
    public IReadOnlyList<ProfileInterestModel> Interests { get; init; } = Array.Empty<ProfileInterestModel>();
    public IReadOnlyList<ProfileHobbyModel> Hobbies { get; init; } = Array.Empty<ProfileHobbyModel>();
    public IReadOnlyList<string> DefaultChatLines { get; init; } = Array.Empty<string>();

    public static ProfileModel FromUser(UserDto.CurrentUser user, string memberSince)
    {
        var interests = user.Interests
            .Select(i =>
            {
                bool isLike = i.Type.Equals(SentimentTypeDto.Like);
                if (isLike)
                {
                    return new ProfileInterestModel(i.Type.ToString(), i.Text, string.Empty);
                }
                return new ProfileInterestModel(i.Type.ToString(), string.Empty, i.Text);
            })
            .ToList();

        var hobbies = user.Hobbies
            .Select(h => new ProfileHobbyModel(h.Hobby.ToString(), h.Name, h.Emoji))
            .ToList();

        return new ProfileModel
        {
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Bio = user.Biography,
            Gender = user.Gender,
            BirthDay = user.BirthDay,
            AvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? DefaultAvatar : user.AvatarUrl,
            MemberSince = memberSince,
            Interests = interests,
            Hobbies = hobbies,
            DefaultChatLines = user.DefaultChatLines
        };
    }

    public const string DefaultAvatar = "https://images.unsplash.com/photo-1531123897727-8f129e1688ce?q=80&w=256&auto=format&fit=crop";
}

public class ProfileDraft
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public GenderTypeDto Gender { get; set; }
    public DateOnly BirthDay { get; set; }
    public string AvatarUrl { get; set; } = ProfileModel.DefaultAvatar;
    public string MemberSince { get; set; } = string.Empty;

    public static ProfileDraft FromModel(ProfileModel model) => new()
    {
        FirstName = model.FirstName,
        LastName = model.LastName,
        Email = model.Email,
        Bio = model.Bio,
        Gender = model.Gender,
        BirthDay = model.BirthDay,
        AvatarUrl = model.AvatarUrl,
        MemberSince = model.MemberSince
    };

    public ProfileModel ApplyTo(ProfileModel original) => original with
    {
        FirstName = FirstName,
        LastName = LastName,
        Email = Email,
        Bio = Bio,
        Gender = Gender,
        BirthDay = BirthDay,
        AvatarUrl = AvatarUrl,
        MemberSince = MemberSince
    };
}
