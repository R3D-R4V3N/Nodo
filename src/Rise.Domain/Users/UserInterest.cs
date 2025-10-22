using Ardalis.GuardClauses;
using Rise.Domain.Common;

namespace Rise.Domain.Users;

public class UserInterest : ValueObject
{
    private string _type = string.Empty;
    private string? _like;
    private string? _dislike;

    public required string Type
    {
        get => _type;
        set => _type = Guard.Against.NullOrWhiteSpace(value);
    }

    public string? Like
    {
        get => _like;
        set => _like = NormalizeOptional(value);
    }

    public string? Dislike
    {
        get => _dislike;
        set => _dislike = NormalizeOptional(value);
    }

    public static UserInterest Create(string type, string? like, string? dislike)
        => new()
        {
            Type = type,
            Like = like,
            Dislike = dislike
        };

    private static string? NormalizeOptional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return Type.ToLowerInvariant();
        yield return Like?.ToLowerInvariant();
        yield return Dislike?.ToLowerInvariant();
    }
}
