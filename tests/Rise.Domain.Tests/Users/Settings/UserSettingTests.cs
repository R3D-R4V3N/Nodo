using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Users.Settings;
using Rise.Tests.Shared;

namespace Rise.Domain.Tests.Users.Settings;

public class UserSettingTests
{
    [Fact]
    public void AddChatTextLine_ShouldAddNewSuggestion()
    {
        var setting = DomainData.ValidUserSettings();
        var line = "Cowabunga.";

        var result = setting.AddChatTextLine(line);

        result.IsSuccess.ShouldBeTrue();
        setting.ChatTextLineSuggestions.ShouldHaveSingleItem();
        setting.ChatTextLineSuggestions[0].Sentence.Value.ShouldBe(line);
        setting.ChatTextLineSuggestions[0].Rank.ShouldBe(0);
    }

    [Fact]
    public void AddChatTextLine_ShouldReturnConflict_WhenDuplicate()
    {
        var setting = DomainData.ValidUserSettings();
        var line = "Cowabunga.";
        setting.AddChatTextLine(line);

        var result = setting.AddChatTextLine(line);

        result.Status.ShouldBe(ResultStatus.Conflict);
        setting.ChatTextLineSuggestions.ShouldHaveSingleItem();
    }

    [Fact]
    public void AddChatTextLine_ShouldReturnConflict_WhenExceedingMax()
    {
        var setting = DomainData.ValidUserSettings();
        int n = UserSetting.MAX_DEFAULT_CHAT_LINE_COUNT;

        for (int i = 0; i < n; i++)
        {
            setting.AddChatTextLine($"Line {i}");
        }

        var result = setting.AddChatTextLine("Extra line");

        result.Status.ShouldBe(ResultStatus.Conflict);
        setting.ChatTextLineSuggestions.Count.ShouldBe(n);
    }

    [Fact]
    public void AddChatTextLine_ShouldInsertAtCorrectRank()
    {
        var setting = DomainData.ValidUserSettings();
        setting.AddChatTextLine("Line 0");
        setting.AddChatTextLine("Line 1");
        setting.AddChatTextLine("Line 2");

        var result = setting.AddChatTextLine("Inserted line", 1);

        result.IsSuccess.ShouldBeTrue();

        setting.ChatTextLineSuggestions[1].Rank.ShouldBe(1);
        setting.ChatTextLineSuggestions[1].Sentence.Value.ShouldBe("Inserted line");

        setting.ChatTextLineSuggestions[2].Rank.ShouldBe(2);
        setting.ChatTextLineSuggestions[2].Sentence.Value.ShouldBe("Line 1");

        setting.ChatTextLineSuggestions.Count.ShouldBe(4);
    }

    [Fact]
    public void RemoveChatTextLine_ShouldRemoveExistingSuggestion()
    {
        var setting = DomainData.ValidUserSettings();
        setting.AddChatTextLine("Line 0");
        setting.AddChatTextLine("Line 1");
        setting.AddChatTextLine("Line 2");

        var line1 = new UserSettingChatTextLineSuggestion()
        {
            Rank = 1,
            Sentence = DefaultSentence.Create("Line 1"),
        };

        var result = setting.RemoveChatTextLine("Line 1");

        result.IsSuccess.ShouldBeTrue();

        setting.ChatTextLineSuggestions.ShouldNotContain(line1);

        setting.ChatTextLineSuggestions[0].Rank.ShouldBe(0);
        setting.ChatTextLineSuggestions[1].Rank.ShouldBe(1);
        setting.ChatTextLineSuggestions.Count.ShouldBe(2);
    }

    [Fact]
    public void RemoveChatTextLine_ShouldReturnConflict_WhenNotFound()
    {
        var setting = DomainData.ValidUserSettings();
        setting.AddChatTextLine("Existing");

        var result = setting.RemoveChatTextLine("Missing");

        result.Status.ShouldBe(ResultStatus.Conflict);
        setting.ChatTextLineSuggestions.ShouldHaveSingleItem();
    }

    [Fact]
    public void SettingUser_ShouldLinkBackToUserSettings()
    {
        var user = DomainData.ValidUser(1);
        var setting = DomainData.ValidUserSettings();

        setting.User = user;

        setting.ShouldBe(setting);

        setting.User.ShouldBe(user);
        setting.User.UserSettings.ShouldBe(setting);
    }

    [Fact]
    public void SettingUser_ShouldIgnoreIfAlreadyAssigned()
    {
        var user = DomainData.ValidUser(1);
        var setting = DomainData.ValidUserSettings();

        setting.User = user;
        var originalSettings = user.UserSettings;

        // Setting again should not cause recursion or replacement
        setting.User = user;

        user.UserSettings.ShouldBe(originalSettings);
    }
}
