using Ardalis.Result;
using Rise.Domain.Common;

namespace Rise.Domain.Common.ValueObjects;
public class Email : ValueObject, IProperty<Email, string>
{
    // EF
    private Email() { }
    public const int MAX_LENGTH = 255;

    public string Value { get; private set; }
    public static Result<Email> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Email is leeg.");
        }

        var cleanedUpMail = value.Trim();

        if (cleanedUpMail.Length > MAX_LENGTH)
        {
            return Result.Conflict($"Email is te lang. Maximum {MAX_LENGTH} tekens.");
        }

        return Result.Success(new Email() { Value = cleanedUpMail });
    }

    public static implicit operator string(Email email) => email.Value;
    public static explicit operator Email(string value)
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