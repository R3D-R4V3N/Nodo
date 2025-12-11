using System;
using Microsoft.AspNetCore.Components;
using Rise.Client.Chats.Components;
using Xunit.Abstractions;

namespace Rise.Client.Tests.Chats.Components;

public class MessageBubbleShould : TestContext
{
    public MessageBubbleShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
    }

    [Fact]
    public void RenderIncomingTextBubble()
    {
        var text = "Hello there";
        var avatarUrl = "avatar.png";

        var cut = RenderComponent<IncomingTextBubble>(parameters => parameters
            .Add(p => p.Text, text)
            .Add(p => p.AvatarUrl, avatarUrl)
        );

        Assert.Contains(text, cut.Markup);
        var img = cut.Find("img");
        Assert.Equal(avatarUrl, img.GetAttribute("src"));
        Assert.Contains("bg-white", cut.Markup);
    }

    [Fact]
    public void RenderOutgoingTextBubblePendingAllowsCancel()
    {
        var text = "Pending message";
        var cancelled = false;

        var cut = RenderComponent<OutgoingTextBubble>(parameters => parameters
            .Add(p => p.Text, text)
            .Add(p => p.IsPending, true)
            .Add(p => p.OnCancelPending, EventCallback.Factory.Create(this, () => cancelled = true))
        );

        var bubble = cut.Find("div[role=\"button\"]");
        Assert.Contains("border-dashed", bubble.GetAttribute("class"));
        bubble.Click();
        Assert.True(cancelled);
        Assert.NotEmpty(cut.FindAll("svg.text-red-600"));
    }

    [Fact]
    public void RenderIncomingAudioBubble()
    {
        var audioUrl = "voice.mp3";
        var duration = TimeSpan.FromSeconds(10);
        var text = "Audio note";

        var cut = RenderComponent<IncomingAudioBubble>(parameters => parameters
            .Add(p => p.AudioUrl, audioUrl)
            .Add(p => p.AudioDuration, duration)
            .Add(p => p.Text, text)
        );

        var audio = cut.Find("audio");
        Assert.Equal(audioUrl, audio.GetAttribute("src"));
        Assert.Contains("0:10", cut.Markup);
        Assert.Contains(text, cut.Markup);
    }

    [Fact]
    public void RenderOutgoingAudioBubblePendingShowsWarning()
    {
        var audioUrl = "clip.mp3";
        var duration = TimeSpan.FromSeconds(42);

        var cut = RenderComponent<OutgoingAudioBubble>(parameters => parameters
            .Add(p => p.AudioUrl, audioUrl)
            .Add(p => p.AudioDuration, duration)
            .Add(p => p.IsPending, true)
        );

        Assert.NotEmpty(cut.FindAll("svg.text-red-600"));
        Assert.Contains("border-dashed", cut.Markup);
    }
}
