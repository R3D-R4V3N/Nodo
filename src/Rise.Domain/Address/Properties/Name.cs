using Ardalis.Result;
using System.Globalization;

namespace Rise.Domain.Locations.Properties;
public class Name : ValueObject, IProperty<Name, string>
{
    // ef
    private Name() { }
    public string Value { get; private set; }
    public const int MAX_LENGTH = 50;

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Naam is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Naam is te lang.");
        }

        var cleanedUpName = CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(value.Trim().ToLower());

        return Result.Success(new Name() { Value = string.Join(' ', cleanedUpName) });
    }

    public static implicit operator string(Name value) => value.Value;

    public static explicit operator Name(string value)
    {
        var result = Create(value);
        if (!result.IsSuccess)
        {
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));
        }

        return result.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }

    public override string ToString() => Value;
}
