namespace Rise.Client.Profile.Models;

public record InterestOption(string Id, string Name, string Emoji);

public record ProfileModel
{
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Bio { get; init; } = string.Empty;
    public string Gender { get; init; } = "x";
    public string AvatarUrl { get; init; } = DefaultAvatar;
    public string MemberSince { get; init; } = "Actief sinds jan. 2024";

    public ProfileModel Clone() => this with { };

    public static ProfileModel CreateDefault() => new()
    {
        Name = "Jouw Naam",
        Email = "jij@example.com",
        Bio = "Kort over mij: ik hou van wandelen, koken en bordspellen.",
        Gender = "x",
        AvatarUrl = DefaultAvatar,
        MemberSince = "Actief sinds jan. 2024"
    };

    public const string DefaultAvatar = "https://images.unsplash.com/photo-1531123897727-8f129e1688ce?q=80&w=256&auto=format&fit=crop";
}

public class ProfileDraft
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Bio { get; set; } = string.Empty;
    public string Gender { get; set; } = "x";
    public string AvatarUrl { get; set; } = ProfileModel.DefaultAvatar;
    public string MemberSince { get; set; } = "Actief sinds jan. 2024";

    public static ProfileDraft FromModel(ProfileModel model) => new()
    {
        Name = model.Name,
        Email = model.Email,
        Bio = model.Bio,
        Gender = model.Gender,
        AvatarUrl = model.AvatarUrl,
        MemberSince = model.MemberSince
    };

    public ProfileModel ToModel() => new()
    {
        Name = Name,
        Email = Email,
        Bio = Bio,
        Gender = Gender,
        AvatarUrl = AvatarUrl,
        MemberSince = MemberSince
    };
}
