using Ardalis.Result;

namespace Rise.Domain.Common.ValueObjects;
public class Biography : ValueObject, IProperty<Biography, string>
{
    // EF
    private Biography() { }
    public const int MAX_LENGTH = 500;

    public string Value { get; private set; }

    public static Result<Biography> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Biografie is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict("Biografie is te lang.");
        }

        return Result.Success(new Biography() { Value = value });
    }

    public static implicit operator string(Biography biography) => biography.Value;
    public static explicit operator Biography(string value)
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