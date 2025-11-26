using Rise.Domain.Common.ValueObjects;

namespace Rise.Domain.Users.Settings;

public class UserSettingChatTextLineSuggestion
{
    public UserSettingChatTextLineSuggestion() { }
    public required int Rank { get; set; }
    public required DefaultSentence Sentence { get; set; }
}