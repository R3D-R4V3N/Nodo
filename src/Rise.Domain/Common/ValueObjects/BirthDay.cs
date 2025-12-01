using Ardalis.Result;
using Rise.Domain.Common;

namespace Rise.Domain.Common.ValueObjects;
public class BirthDay : ValueObject, IProperty<BirthDay, DateOnly>
{
    // EF
    private BirthDay() { }

    public DateOnly Value { get; private set; }
    public static Result<BirthDay> Create(DateOnly value)
    {
        if (value > DateOnly.FromDateTime(DateTime.UtcNow))
        {
            return Result.Conflict("Verjaardag ligt in de toekomst.");
        }

        return Result.Success(new BirthDay() { Value = value });
    }

    public static implicit operator DateOnly(BirthDay bday) => bday.Value;
    public static explicit operator BirthDay(DateOnly value)
    {
        var result = Create(value);
        if (!result.IsSuccess)
        {
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));
        }

        return result.Value;
    }

    public override string ToString() => Value.ToShortDateString();

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
}