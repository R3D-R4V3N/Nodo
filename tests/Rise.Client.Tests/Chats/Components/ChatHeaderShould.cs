using Microsoft.AspNetCore.Components;
using Rise.Client.Chats.Components;
using Rise.Shared.Assets;
using Xunit.Abstractions;

namespace Rise.Client.Tests.Chats.Components;

public class ChatHeaderShould : TestContext
{
    public ChatHeaderShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
    }

    [Fact]
    public void RenderPropertiesCorrectly()
    {
        // Arrange
        var displayName = "Team Chat";
        var avatarUrl = "https://example.com/avatar.jpg";
        var statusText = "Active now";

        // Act
        var cut = RenderComponent<ChatHeader>(parameters => parameters
            .Add(p => p.DisplayName, displayName)
            .Add(p => p.AvatarUrl, avatarUrl)
            .Add(p => p.StatusText, statusText)
        );

        // Assert
        // Check TopBar Title (TopBar renders its Title parameter)
        // Since TopBar is a custom component, we can look for the text rendered inside it.
        cut.WaitForAssertion(() => Assert.Contains(displayName, cut.Markup));
        
        // Check Subtitle
        Assert.Contains(statusText, cut.Markup);

        // Check Avatar
        var img = cut.Find("img");
        Assert.Equal(avatarUrl, img.GetAttribute("src"));
        Assert.Equal(displayName, img.GetAttribute("alt"));
    }

    [Fact]
    public async Task TriggerOnTopBarClick()
    {
        // Arrange
        var clicked = false;
        var cut = RenderComponent<ChatHeader>(parameters => parameters
            .Add(p => p.OnTopBarClick, EventCallback.Factory.Create(this, () => clicked = true))
        );

        // Act
        // TopBar passes the onclick to its root element or a specific clickable area.
        // Assuming TopBar renders a div/header with the onclick.
        // Let's find the TopBar component instance and verify parameters or simulate click if possible.
        // Since TopBar is wrapped, we might just click the first child or inspect markup.
        // However, looking at ChatHeader.razor: <TopBar ... onclick="@OnTopBarClick">
        // We can try finding the element TopBar renders.
        
        // Let's click the main element
        // TopBar renders a div (or header) with the onclick attribute splatted onto it.
        // We can find the root element of the TopBar.
        var topBarRoot = cut.Find("div.rounded-b-3xl"); // Using a class specific to TopBar
        topBarRoot.Click();

        // Assert
        Assert.True(clicked);
    }

    [Fact]
    public async Task TriggerOnAlert()
    {
        // Arrange
        var alerted = false;
        var cut = RenderComponent<ChatHeader>(parameters => parameters
            .Add(p => p.OnAlert, EventCallback.Factory.Create(this, () => alerted = true))
        );

        // Act
        // Use a simpler selector or filter
        var alertButton = cut.FindAll("button").FirstOrDefault(b => b.ClassName != null && b.ClassName.Contains("bg-[#D64541]"));
        Assert.NotNull(alertButton);
        alertButton.Click();

        // Assert
        Assert.True(alerted);
    }
}
