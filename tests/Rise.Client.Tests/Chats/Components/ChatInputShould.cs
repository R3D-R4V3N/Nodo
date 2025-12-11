using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Rise.Client.Chats.Components;
using Xunit.Abstractions;

namespace Rise.Client.Tests.Chats.Components;

public class ChatInputShould : TestContext
{
    public ChatInputShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
        Services.AddClientTestDefaults();
        JSInterop.Mode = JSRuntimeMode.Loose; // Allow unmocked JS calls (mostly)
    }

    [Fact]
    public void BindValueCorrectly()
    {
        // Arrange
        var value = "Initial";
        
        // Act
        var cut = RenderComponent<ChatInput>(parameters => parameters
            .Add(p => p.Value, value)
        );

        // Assert
        var input = cut.Find("input");
        Assert.Equal(value, input.Attributes["value"]?.Value);
    }

    [Fact]
    public void UpdateValueOnInput()
    {
        // Arrange
        var cut = RenderComponent<ChatInput>(parameters => parameters
            .Add(p => p.Value, "")
        );

        // Act
        var input = cut.Find("input");
        input.Input("New Value");

        // Assert
        Assert.Equal("New Value", cut.Instance.Value);
    }

    [Fact]
    public async Task TriggerOnSendWhenSubmitted()
    {
        // Arrange
        var sentText = "";
        var cut = RenderComponent<ChatInput>(parameters => parameters
            .Add(p => p.Value, "Hello World")
            .Add(p => p.OnSend, EventCallback.Factory.Create<string>(this, s => sentText = s))
        );

        // Act
        await cut.Find("form").SubmitAsync();

        // Assert
        Assert.Equal("Hello World", sentText);
    }

    [Fact]
    public void DisableSendButtonWhenEmpty()
    {
        // Arrange
        var cut = RenderComponent<ChatInput>(parameters => parameters
            .Add(p => p.Value, "")
        );

        // Assert
        var btn = cut.Find("button[type=submit]");
        Assert.True(btn.HasAttribute("disabled"));
    }
}
