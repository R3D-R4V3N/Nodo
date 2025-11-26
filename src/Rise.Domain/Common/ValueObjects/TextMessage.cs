using Ardalis.Result;

namespace Rise.Domain.Common.ValueObjects;
public class TextMessage : ValueObject, IProperty<TextMessage, string>
{
    // ef
    private TextMessage() { }
    public const int MAX_LENGTH = 2_000;

    private string _value;
    public string Value => _value;
    private string _cleanedUpValue;
    public string CleanedUpValue 
    { 
        get 
        {
            if (string.IsNullOrWhiteSpace(_cleanedUpValue))
                _cleanedUpValue = IsSuspicious ? WordFilter.Censor(_value) : _value;

            return _cleanedUpValue;
        }
    }
    public bool IsSuspicious { get; private set; }
    public static Result<TextMessage> Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result.Conflict("Bericht is leeg.");
        }

        var cleanedUpValue = value.Trim();

        if (cleanedUpValue.Length > MAX_LENGTH)
        {
            return Result.Conflict("Bericht is te lang.");
        }
        
        return Result.Success(new TextMessage() 
        { 
            _value = cleanedUpValue,
            IsSuspicious = WordFilter.ContainsBlackListedWord(cleanedUpValue)
        });
    }

    public static implicit operator string(TextMessage value) => value.Value;

    public static explicit operator TextMessage(string value)
    {
        var result = Create(value);
        if (!result.IsSuccess)
        {
            throw new ArgumentException(string.Join(',', result.Errors), nameof(value));
        }

        return result.Value;
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return Value;
    }
    public override string ToString() => CleanedUpValue;
}