using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Rise.Shared.Profile;

namespace Rise.Client.Profile.Models;

public record InterestOption(string Id, string Name, string Emoji);

public record ProfileModel
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string Gender { get; init; } = "x";
    public string AvatarUrl { get; init; } = DefaultAvatar;
    public DateTime MemberSince { get; init; } = DateTime.UtcNow;
    public IReadOnlyList<string> Interests { get; init; } = Array.Empty<string>();

    public string DisplayName => string.Join(" ", new[] { FirstName, LastName }.Where(x => !string.IsNullOrWhiteSpace(x))).Trim();

    public string MemberSinceDisplay
    {
        get
        {
            var culture = new CultureInfo("nl-BE");
            return $"Actief sinds {MemberSince.ToString("MMM yyyy", culture)}";
        }
    }

    public ProfileRequest.UpdateProfile ToUpdateRequest(IEnumerable<string> interests) => new()
    {
        FirstName = FirstName,
        LastName = LastName,
        Email = Email,
        Biography = Bio,
        Gender = Gender,
        AvatarUrl = AvatarUrl,
        Interests = interests.Select(id => id).ToList()
    };

    public static ProfileModel FromResponse(ProfileResponse.Profile profile) => new()
    {
        FirstName = profile.FirstName,
        LastName = profile.LastName,
        Email = profile.Email,
        Bio = profile.Biography,
        Gender = profile.Gender,
        AvatarUrl = string.IsNullOrWhiteSpace(profile.AvatarUrl) ? DefaultAvatar : profile.AvatarUrl,
        MemberSince = profile.MemberSince,
        Interests = profile.Interests?.ToList() ?? new List<string>()
    };

    public static ProfileModel CreateDefault() => new()
    {
        FirstName = "Jouw",
        LastName = "Naam",
        Email = "jij@example.com",
        Bio = "Kort over mij: ik hou van wandelen, koken en bordspellen.",
        Gender = "x",
        AvatarUrl = DefaultAvatar,
        MemberSince = DateTime.UtcNow,
        Interests = Array.Empty<string>()
    };

    public const string DefaultAvatar = "https://images.unsplash.com/photo-1531123897727-8f129e1688ce?q=80&w=256&auto=format&fit=crop";
}

public class ProfileDraft
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Gender { get; set; } = "x";
    public string AvatarUrl { get; set; } = ProfileModel.DefaultAvatar;

    public static ProfileDraft FromModel(ProfileModel model) => new()
    {
        FullName = model.DisplayName,
        Email = model.Email,
        Bio = model.Bio,
        Gender = model.Gender,
        AvatarUrl = model.AvatarUrl
    };

    public ProfileRequest.UpdateProfile ToUpdateRequest(ProfileModel current, IEnumerable<string> interests)
    {
        var (firstName, lastName) = ParseName(FullName, current);
        var email = string.IsNullOrWhiteSpace(Email) ? current.Email : Email.Trim();
        var bio = string.IsNullOrWhiteSpace(Bio) ? current.Bio : Bio.Trim();
        if (string.IsNullOrWhiteSpace(bio))
        {
            bio = current.Bio;
        }

        var gender = string.IsNullOrWhiteSpace(Gender)
            ? current.Gender
            : NormalizeGender(Gender, current.Gender);

        var avatar = string.IsNullOrWhiteSpace(AvatarUrl)
            ? current.AvatarUrl
            : AvatarUrl.Trim();

        if (string.IsNullOrWhiteSpace(avatar))
        {
            avatar = current.AvatarUrl;
        }

        return new ProfileRequest.UpdateProfile
        {
            FirstName = firstName,
            LastName = lastName,
            Email = string.IsNullOrWhiteSpace(email) ? current.Email : email,
            Biography = bio,
            Gender = gender,
            AvatarUrl = avatar,
            Interests = interests.Select(id => id).ToList()
        };
    }

    private static (string firstName, string lastName) ParseName(string? fullName, ProfileModel current)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (current.FirstName, current.LastName);
        }

        var parts = fullName.Split(' ', 2, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 1)
        {
            var first = parts[0];
            var last = !string.IsNullOrWhiteSpace(current.LastName) ? current.LastName : parts[0];
            return (first, last);
        }

        return (parts[0], parts[1]);
    }

    private static string NormalizeGender(string value, string fallback)
    {
        var normalized = value.Trim().ToLowerInvariant();
        return normalized switch
        {
            "man" or "vrouw" or "x" => normalized,
            _ => fallback
        };
    }
}
