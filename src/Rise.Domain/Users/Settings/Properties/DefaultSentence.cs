using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Users.Settings.Properties;
public class DefaultSentence : ValueObject, IProperty<DefaultSentence, string>
{
    // ef
    private DefaultSentence() { }
    public const int MAX_LENGTH = 150;

    public string Value { get; private set; }
    public static Result<DefaultSentence> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Standaardzin is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Standaardzin is te lang.");
        }

        return Result.Success(new DefaultSentence() { Value = value });
    }

    public static implicit operator string(DefaultSentence value) => value.Value;

    public static explicit operator DefaultSentence(string value)
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