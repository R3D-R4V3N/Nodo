using Microsoft.AspNetCore.Components;
using Rise.Client.Chats.Components;
using Xunit.Abstractions;

namespace Rise.Client.Tests.Chats.Components;

public class SuggestionChipsShould : TestContext
{
    public SuggestionChipsShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
    }

    [Fact]
    public void RenderSuggestions()
    {
        // Arrange
        var suggestions = new List<string> { "Yes", "No", "Maybe" };

        // Act
        var cut = RenderComponent<SuggestionChips>(parameters => parameters
            .Add(p => p.Suggestions, suggestions)
        );

        // Assert
        var buttons = cut.FindAll("button");
        Assert.Equal(3, buttons.Count);
        Assert.Contains("Yes", buttons[0].TextContent);
        Assert.Contains("No", buttons[1].TextContent);
        Assert.Contains("Maybe", buttons[2].TextContent);
    }

    [Fact]
    public async Task TriggerOnPickWhenClicked()
    {
        // Arrange
        var suggestions = new List<string> { "Hello" };
        var picked = "";
        
        var cut = RenderComponent<SuggestionChips>(parameters => parameters
            .Add(p => p.Suggestions, suggestions)
            .Add(p => p.OnPick, EventCallback.Factory.Create<string>(this, s => picked = s))
        );

        // Act
        cut.Find("button").Click();

        // Assert
        Assert.Equal("Hello", picked);
    }
}
