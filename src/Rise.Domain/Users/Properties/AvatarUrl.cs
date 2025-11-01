using Ardalis.Result;
using Rise.Domain.Common;

namespace Rise.Domain.Users.Properties;
public class AvatarUrl : ValueObject, IProperty<AvatarUrl, string>
{
    // EF
    private AvatarUrl() { }
    public const int MAX_LENGTH = 150;

    public string Value { get; private set; }
    public static Result<AvatarUrl> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Avatar url is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Avatar url is te lang.");
        }

        return Result.Success(new AvatarUrl() { Value = value });
    }

    public static implicit operator string(AvatarUrl avatarUrl) => avatarUrl.Value;
    public static explicit operator AvatarUrl(string value)
    {
        var result = Create(value);
        if (!result.IsSuccess)
        {
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));
        }

        return result.Value;
    }

    public override string ToString() => Value;

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}