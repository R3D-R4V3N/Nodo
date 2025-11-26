using Ardalis.Result;

namespace Rise.Domain.Common.ValueObjects;

public class RegistrationNote : ValueObject, IProperty<RegistrationNote, string>
{
    // EF
    private RegistrationNote() { }
    public const int MAX_LENGTH = 200;

    public string Value { get; private set; }

    public static Result<RegistrationNote> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Registratie notitie is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Registratie notitie is te lang.");
        }

        return Result.Success(new RegistrationNote() { Value = value });
    }

    public static implicit operator string(RegistrationNote biography) => biography.Value;
    public static explicit operator RegistrationNote(string value)
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
