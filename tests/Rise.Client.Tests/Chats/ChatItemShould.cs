using Rise.Client.Chats;
using Xunit.Abstractions;

namespace Rise.Client.Tests.Chats;

public class ChatItemShould : TestContext
{
    public ChatItemShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
    }

    [Fact]
    public void RenderPropertiesCorrectly()
    {
        // Arrange
        var name = "Kyandro";
        var message = "Hello World";
        var time = "10:00";

        // Act
        var cut = RenderComponent<ChatItem>(parameters => parameters
            .Add(p => p.Name, name)
            .Add(p => p.Message, message)
            .Add(p => p.Time, time)
        );

        // Assert
        cut.Find("p.font-semibold").MarkupMatches($"<p class=\"font-semibold text-gray-900\">{name}</p>");
        cut.Find("p.text-gray-500").MarkupMatches($"<p class=\"text-gray-500 truncate\">{message}</p>");
        cut.Find("div.text-sm.text-gray-400").MarkupMatches($"<div class=\"text-sm text-gray-400\">{time}</div>");
    }
}