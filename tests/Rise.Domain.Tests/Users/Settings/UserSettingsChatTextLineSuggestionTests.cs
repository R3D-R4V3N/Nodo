using Rise.Domain.Users.Settings;
using Rise.Tests.Shared;

namespace Rise.Domain.Tests.Users.Settings;

public class UserSettingsChatTextLineSuggestionTests
{
    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    [InlineData(1)]
    public void Constructor_ShouldCreateInstance_WhenRequiredPropertiesAreSet(int rank)
    {
        var defaultSentence = DomainData.ValidDefaultSentence();

        var suggestion = new UserSettingChatTextLineSuggestion
        {
            Rank = rank,
            Sentence = defaultSentence
        };

        suggestion.Rank.ShouldBe(rank);
        suggestion.Sentence.ShouldBe(defaultSentence);
    }

    [Fact]
    public void SettingProperties_ShouldUpdateValues()
    {
        var sentence1 = DomainData.ValidDefaultSentence();
        var sentence2 = DomainData.ValidDefaultSentence(1);
        var suggestion = new UserSettingChatTextLineSuggestion
        {
            Rank = 1,
            Sentence = sentence1
        };

        suggestion.Rank = 2;
        suggestion.Sentence = sentence2;

        suggestion.Rank.ShouldBe(2);
        suggestion.Sentence.ShouldBe(sentence2);
    }
}
