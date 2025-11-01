using Ardalis.Result;

namespace Rise.Domain.Organizations.Properties;

public sealed class Name : ValueObject, IProperty<Name, string>
{
    private Name() { }

    public const int MAX_LENGTH = 200;

    public string Value { get; private set; } = default!;

    public static Result<Name> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Organisatienaam is leeg.");
        }

        var trimmedValue = value.Trim();

        if (trimmedValue.Length > MAX_LENGTH)
        {
            return Result.Conflict("Organisatienaam is te lang.");
        }

        return Result.Success(new Name
        {
            Value = trimmedValue,
        });
    }

    public static implicit operator string(Name name) => name.Value;

    public static explicit operator Name(string value)
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
