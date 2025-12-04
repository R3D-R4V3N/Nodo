using Ardalis.Result;

namespace Rise.Domain.Common.ValueObjects;
public class EmergencyRange : ValueObject
{
    // EF
    private EmergencyRange() { }
    private const int RANGE_OFFSET_HOURS = 24;

    public DateTime Start => End.AddHours(-RANGE_OFFSET_HOURS);
    public DateTime End { get; private set; }
    public static Result<EmergencyRange> Create()
    {
        return Result.Success(new EmergencyRange() { End = DateTime.UtcNow });
    }

    public static Result<EmergencyRange> Create(DateTime happenedAt)
    {
        if (happenedAt > DateTime.UtcNow)
        {
            return Result.Conflict("Noodmelding ligt in de toekomst.");
        }

        var currentDatetime = DateTime.UtcNow;
        var max = happenedAt < currentDatetime
            ? happenedAt : currentDatetime;

        return Result.Success(new EmergencyRange() { End = max });
    }

    public static implicit operator DateTime(EmergencyRange range) => range.End;
    public static explicit operator EmergencyRange(DateTime value)
    {
        var result = Create(value);
        if (!result.IsSuccess)
        {
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));
        }

        return result.Value;
    }

    public override string ToString() => $"{Start.ToShortDateString()}-{End.ToShortDateString()}";

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return End;
    }
}