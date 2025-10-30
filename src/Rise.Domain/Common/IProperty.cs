using Ardalis.Result;

namespace Rise.Domain.Common;
public interface IProperty<TSelf, TValue> 
    where TSelf : IProperty<TSelf, TValue>
{
    TValue Value { get; }
    static abstract Result<TSelf> Create(TValue value);
    static abstract implicit operator TValue(TSelf value);
    static abstract explicit operator TSelf(TValue value);
}