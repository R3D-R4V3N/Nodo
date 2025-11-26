using Ardalis.Result;
using System.Globalization;

namespace Rise.Domain.Common.ValueObjects;
public class FirstName : ValueObject, IProperty<FirstName, string>
{
    // EF
    private FirstName() { }
    public const int MAX_LENGTH = 100;

    public string Value { get; private set; }

    public static Result<FirstName> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Voornaam is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Voornaam is te lang.");
        }

        var cleanedUpName = CultureInfo.CurrentCulture.TextInfo
            .ToTitleCase(value.Trim().ToLower());

        return Result.Success(new FirstName() { Value = string.Join(' ', cleanedUpName) });
    }

    public static implicit operator string(FirstName firstName) => firstName.Value;
    public static explicit operator FirstName(string value)
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