using Ardalis.Result;

namespace Rise.Client.Offline;

public static class ResultExtensions
{
    public static Result<T> MarkCached<T>(this Result<T> result)
    {
        return result;
    }
}
