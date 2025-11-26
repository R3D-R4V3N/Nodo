using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;

namespace Rise.Domain.Users.Settings;

public class UserSetting : ValueObject
{
    public UserSetting() { }
    public const int MAX_DEFAULT_CHAT_LINE_COUNT = 5;

    private BaseUser _user;
    public BaseUser User
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
    public required FontSize FontSize { get; set; }

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

        if (_chatTextLineSuggestions.Any(s => s.Sentence == cleanedUpLine))
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
            Sentence = DefaultSentence.Create(cleanedUpLine),
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
            .FirstOrDefault(s => s.Sentence == cleanedUpLine);

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
}