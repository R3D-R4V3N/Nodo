using Ardalis.Result;

namespace Rise.Domain.Messages.Properties;
public class Text : ValueObject, IProperty<Text, string>
{
    // ef
    private Text() { }
    public const int MAX_LENGTH = 2_000;

    public string Value { get; private set; }
    public static Result<Text> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Bericht is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Bericht is te lang.");
        }

        return Result.Success(new Text() { Value = value });
    }


    public static implicit operator string(Text value) => value.Value;

    public static explicit operator Text(string value)
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