namespace Rise.Domain.Users;

public class UserSettingChatTextLineSuggestion
{
    private const int MAX_TEXT_LINE_LENGTH = 150;

    public required int Rank { get; set; }
    private string _text { get; set; }
    public required string Text 
    { 
        get => _text;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException($"Standaardzin is leeg");
            }
            if (value.Length >= MAX_TEXT_LINE_LENGTH)
            {
                throw new ArgumentException($"Standaardzin mag maximaal {MAX_TEXT_LINE_LENGTH} characters lang zijn");
            }
            _text = value;
        }
    }

    public UserSettingChatTextLineSuggestion()
    {
    }
}