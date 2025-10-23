using System.Collections.Generic;
using System.Linq;
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
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string Gender { get; init; } = "x";
    public string AvatarUrl { get; init; } = DefaultAvatar;
    public string MemberSince { get; init; } = string.Empty;
    public IReadOnlyList<ProfileInterestModel> Interests { get; init; } = Array.Empty<ProfileInterestModel>();
    public IReadOnlyList<ProfileHobbyModel> Hobbies { get; init; } = Array.Empty<ProfileHobbyModel>();

    public static ProfileModel FromUser(UserDto.CurrentUser user, string memberSince)
    {
        var interests = user.Interests
            .Select(i => new ProfileInterestModel(i.Type, i.Like, i.Dislike))
            .ToList();

        var hobbies = user.Hobbies
            .Select(h => new ProfileHobbyModel(h.Id, h.Name, h.Emoji))
            .ToList();

        return new ProfileModel
        {
            Name = user.Name,
            Email = user.Email,
            Bio = user.Biography,
            Gender = "x",
            AvatarUrl = string.IsNullOrWhiteSpace(user.AvatarUrl) ? DefaultAvatar : user.AvatarUrl,
            MemberSince = memberSince,
            Interests = interests,
            Hobbies = hobbies
        };
    }

    public const string DefaultAvatar = "https://images.unsplash.com/photo-1531123897727-8f129e1688ce?q=80&w=256&auto=format&fit=crop";
}

public class ProfileDraft
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Gender { get; set; } = "x";
    public string AvatarUrl { get; set; } = ProfileModel.DefaultAvatar;
    public string MemberSince { get; set; } = string.Empty;

    public static ProfileDraft FromModel(ProfileModel model) => new()
    {
        Name = model.Name,
        Email = model.Email,
        Bio = model.Bio,
        Gender = model.Gender,
        AvatarUrl = model.AvatarUrl,
        MemberSince = model.MemberSince
    };

    public ProfileModel ApplyTo(ProfileModel original) => original with
    {
        Name = Name,
        Email = Email,
        Bio = Bio,
        Gender = Gender,
        AvatarUrl = AvatarUrl,
        MemberSince = MemberSince
    };
}
