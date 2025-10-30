using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Rise.Domain.Common;

public class ValueObjectConverter<TProperty, TValue>
    : ValueConverter<TProperty, TValue>
    where TProperty : IProperty<TProperty, TValue>
{
    public ValueObjectConverter()
        : base(
            v => v.Value,
            v => CreateProperty(v)
        )
    {
    }

    private static TProperty CreateProperty(TValue value)
    {
        var result = TProperty.Create(value);
        if (!result.IsSuccess)
        {
            throw new InvalidOperationException(
                $"Failed to create {typeof(TProperty).Name}: {string.Join(',', result.Errors)}"
            );
        }

        return result.Value;
    }
}
