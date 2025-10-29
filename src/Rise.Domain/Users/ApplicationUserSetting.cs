using Ardalis.Result;

namespace Rise.Domain.Users;

public class ApplicationUserSetting : ValueObject
{
    private const int MIN_LETTER_SIZE = 10,
        MAX_LETTER_SIZE = 30,
        MAX_DEFAULT_CHAT_LINE_COUNT = 5;

    private ApplicationUser _user;
    public ApplicationUser User
    {
        get => _user;
        set
        {
            if (_user == value) return;

            _user = Guard.Against.Null(value);
            if (_user.UserSettings != this)
            {
                _user.UserSettings = this;
            }
        }
    }
    public bool IsDarkMode { get; set; } = false;
    public int _fontSize;
    public required int FontSize 
    { 
        get => _fontSize;
        set
        {
            if (value < MIN_LETTER_SIZE || value > MAX_LETTER_SIZE)
            {
                throw new ArgumentException(
                    $"Lettergrootte moet tussen {MIN_LETTER_SIZE} en {MAX_LETTER_SIZE} zijn"
                );
            }

            _fontSize = value;
        }
    }

    private readonly List<UserSettingChatTextLineSuggestion> _chatTextLineSuggestions = [];
    public IReadOnlyList<UserSettingChatTextLineSuggestion> ChatTextLineSuggestions 
        => _chatTextLineSuggestions.AsReadOnly();

    public Result AddChatTextLine(string line, int rank = -1)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return Result.Conflict($"standaardzin is leeg");
        }

        string cleanedUpLine = line.Trim();

        if (_chatTextLineSuggestions.Any(s => s.Text == cleanedUpLine))
        { 
            return Result.Conflict($"{cleanedUpLine} zit al tussen de standaardzinnen");
        }

        if (_chatTextLineSuggestions.Count >= MAX_DEFAULT_CHAT_LINE_COUNT)
        { 
            return Result.Conflict($"Gebruiker mag maximaal {MAX_DEFAULT_CHAT_LINE_COUNT} chat standaardzinnen hebben");
        }

        rank = rank == -1 ? _chatTextLineSuggestions.Count : rank;

        var newSuggestion = new UserSettingChatTextLineSuggestion()
        {
            Rank = rank,
            Text = cleanedUpLine
        };

        _chatTextLineSuggestions.Insert(
            rank,
            newSuggestion
        );

        for (int i = rank + 1; i < _chatTextLineSuggestions.Count; i++)
        {
            _chatTextLineSuggestions[i].Rank++;
        }

        return Result.Success();
    }

    public Result RemoveChatTextLine(string line)
    {
        string cleanedUpLine = line.Trim();

        var suggestionToRemove = _chatTextLineSuggestions
            .FirstOrDefault(s => s.Text == cleanedUpLine);

        if (suggestionToRemove is null)
        {
            return Result.Conflict($"{cleanedUpLine} zit niet tussen de standaardzinnen");
        }

        int removedRank = suggestionToRemove.Rank;
        _chatTextLineSuggestions.Remove(suggestionToRemove);

        for (int i = removedRank; i < _chatTextLineSuggestions.Count; i++)
        {
            _chatTextLineSuggestions[i].Rank--;
        }

        return Result.Success();
    }

    public void RemoveChatTextLines()
    {
        _chatTextLineSuggestions.Clear();
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return User;
    }

    public ApplicationUserSetting()
    {
    }
}
