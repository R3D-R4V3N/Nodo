using Ardalis.Result;
using System.Globalization;

namespace Rise.Domain.Users.Properties;
public class LastName : ValueObject, IProperty<LastName, string>
{
    // EF
    private LastName() { }
    public const int MAX_LENGTH = 100;

    public string Value { get; private set; }

    public static Result<LastName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Achternaam is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Achternaam is te lang.");
        }

        var cleanedUpName = CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(value.Trim().ToLower());

        return Result.Success(new LastName() { Value = cleanedUpName });
    }

    public static implicit operator string(LastName lastname) => lastname.Value;
    public static explicit operator LastName(string value)
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