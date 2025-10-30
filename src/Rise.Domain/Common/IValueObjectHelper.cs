namespace Rise.Domain.Common;

// doesnt work atm
public static class IValueObjectHelper<TSelf, TValue>
    where TSelf : IProperty<TSelf, TValue>
{
    public static TValue ToValue(TSelf value)
        => value.Value;

    public static TSelf FromValue(TValue value)
    {
        var result = TSelf.Create(value); 

        if (!result.IsSuccess)
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));

        return result.Value;
    }
}