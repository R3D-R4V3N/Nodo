using Ardalis.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rise.Domain.Users.Properties;
public class FontSize : ValueObject, IProperty<FontSize, int>
{
    // EF
    private FontSize() { }
    public const int MIN_FONT_SIZE = 10;
    public const int MAX_FONT_SIZE = 30;

    public int Value { get; private set; }
    public static Result<FontSize> Create(int value)
    {
        if (value < MIN_FONT_SIZE || value > MAX_FONT_SIZE)
        {
            return Result.Conflict(
                $"Lettergrootte moet tussen {MIN_FONT_SIZE} en {MAX_FONT_SIZE} zijn"
            );
        }

        return Result.Success(new FontSize() { Value = value });
    }

    public static implicit operator int(FontSize fontSize) => fontSize.Value;
    public static explicit operator FontSize(int value)
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