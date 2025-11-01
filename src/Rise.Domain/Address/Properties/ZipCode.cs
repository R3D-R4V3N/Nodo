using Ardalis.Result;

namespace Rise.Domain.Locations.Properties;
public class ZipCode : ValueObject, IProperty<ZipCode, int>
{
    public const int MIN_RANGE = 1000;
    public const int MAX_RANGE = 9992;

    public int Value { get; private set; }

    public static Result<ZipCode> Create(int value)
    {
        if (value < MIN_RANGE || value > MAX_RANGE)
        {
            return Result.Conflict(
                $"Lettergrootte moet tussen {MIN_RANGE} en {MAX_RANGE} zijn"
            );
        }

        return Result.Success(new ZipCode() { Value = value });
    }

    public static implicit operator int(ZipCode value) => value.Value;

    public static explicit operator ZipCode(int value)
    {
        var result = Create(value);
        if (!result.IsSuccess)
        {
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));
        }

        return result.Value;
    }
    public override string ToString() => Value.ToString();
    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}
