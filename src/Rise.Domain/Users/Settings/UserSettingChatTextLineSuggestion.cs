using Rise.Domain.Users.Settings.Properties;

namespace Rise.Domain.Users.Settings;

public class UserSettingChatTextLineSuggestion
{
    public UserSettingChatTextLineSuggestion() { }
    public required int Rank { get; set; }
    public required DefaultSentence Sentence { get; set; }
}