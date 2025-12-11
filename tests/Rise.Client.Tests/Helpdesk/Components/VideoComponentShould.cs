using Xunit.Abstractions;
using Rise.Client.Helpdesk.Components;
using Microsoft.AspNetCore.Components;

namespace Rise.Client.Tests.Helpdesk.Components;

public class VideoComponentShould : TestContext
{
    public VideoComponentShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
    }

    [Fact]
    public void RenderTitleCorrectly()
    {
        // Arrange
        var title = "Test Video Title";
        var url = "https://example.com/video";

        // Act
        var cut = RenderComponent<VideoComponent>(parameters => parameters
            .Add(p => p.Titel, title)
            .Add(p => p.Url, url)
        );

        // Assert
        cut.Find("h2").MarkupMatches($"<h2 class=\"text-gray-800 font-bold text-lg text-center md:text-2xl p-4 \">{title}</h2>");
    }

    [Fact]
    public void RenderIframeWithCorrectSrcForGenericUrl()
    {
        // Arrange
        var url = "https://example.com/video";

        // Act
        var cut = RenderComponent<VideoComponent>(parameters => parameters
            .Add(p => p.Url, url)
        );

        // Assert
        var iframe = cut.Find("iframe");
        Assert.Equal(url, iframe.GetAttribute("src"));
    }

    [Fact]
    public void RenderCorrectEmbedUrlForYoutubeLongForm()
    {
        // Arrange
        var inputUrl = "https://www.youtube.com/watch?v=dQw4w9WgXcQ";
        var expectedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ?rel=0&playsinline=1&modestbranding=1";

        // Act
        var cut = RenderComponent<VideoComponent>(parameters => parameters
            .Add(p => p.Url, inputUrl)
        );

        // Assert
        var iframe = cut.Find("iframe");
        Assert.Equal(expectedUrl, iframe.GetAttribute("src"));
    }

    [Fact]
    public void RenderCorrectEmbedUrlForYoutubeShortForm()
    {
        // Arrange
        var inputUrl = "https://youtu.be/dQw4w9WgXcQ";
        var expectedUrl = "https://www.youtube.com/embed/dQw4w9WgXcQ?rel=0&playsinline=1&modestbranding=1";

        // Act
        var cut = RenderComponent<VideoComponent>(parameters => parameters
            .Add(p => p.Url, inputUrl)
        );

        // Assert
        var iframe = cut.Find("iframe");
        Assert.Equal(expectedUrl, iframe.GetAttribute("src"));
    }

    [Fact]
    public void RenderEmptySrcForEmptyUrl()
    {
        // Arrange
        var url = "";

        // Act
        var cut = RenderComponent<VideoComponent>(parameters => parameters
            .Add(p => p.Url, url)
        );

        // Assert
        var iframe = cut.Find("iframe");
        Assert.Equal("", iframe.GetAttribute("src"));
    }
}
