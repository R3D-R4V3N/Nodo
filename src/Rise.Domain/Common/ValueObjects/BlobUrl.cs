using Ardalis.Result;
using Rise.Domain.Common;

namespace Rise.Domain.Common.ValueObjects;
public class BlobUrl : ValueObject, IProperty<BlobUrl, string>
{
    // EF
    private BlobUrl() { }
    public const int MAX_LENGTH = 200;

    public string Value { get; private set; }
    public static Result<BlobUrl> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Blob url is leeg.");
        }

        if (value.Length > MAX_LENGTH)
        {
            return Result.Conflict($"Blob url is te lang. Maximum {MAX_LENGTH} tekens.");
        }

        return Result.Success(new BlobUrl() { Value = value });
    }

    public static implicit operator string(BlobUrl blobUrl) => blobUrl.Value;
    public static explicit operator BlobUrl(string value)
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