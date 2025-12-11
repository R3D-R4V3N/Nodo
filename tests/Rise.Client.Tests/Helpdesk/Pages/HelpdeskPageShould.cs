using Xunit.Abstractions;
using Rise.Client.Helpdesk.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Rise.Client.Tests.Helpdesk.Pages;

public class HelpdeskPageShould : TestContext
{
    private readonly NavigationManager _navManager;

    public HelpdeskPageShould(ITestOutputHelper outputHelper)
    {
        Services.AddClientTestDefaults();
        // Setup Auth (Page has [Authorize])
        var identity = new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1"),
        }, "TestAuth");
        var user = new ClaimsPrincipal(identity);
        var authProvider = Substitute.For<AuthenticationStateProvider>();
        authProvider.GetAuthenticationStateAsync().Returns(Task.FromResult(new AuthenticationState(user)));
        Services.AddSingleton(authProvider);

        _navManager = Services.GetRequiredService<NavigationManager>();
    }

    [Fact]
    public void RenderMenuButtons()
    {
        // Arrange & Act
        var cut = RenderComponent<HelpdeskPage>();

        // Assert
        // Check for 4 main buttons
        var buttons = cut.FindAll("button.rounded-2xl");
        Assert.Equal(4, buttons.Count);
        
        Assert.Contains("Hulp-Robot", cut.Markup);
        Assert.Contains("Informatie-video's", cut.Markup);
        Assert.Contains("Voorlichting", cut.Markup);
        Assert.Contains("Chatten met begeleider", cut.Markup);
    }

    [Fact]
    public void NavigateToHulpRobot()
    {
        // Arrange
        var cut = RenderComponent<HelpdeskPage>();

        // Act
        // Find button by text content
        var button = cut.FindAll("button").First(b => b.TextContent.Contains("Hulp-Robot"));
        button.Click();

        // Assert
        Assert.Equal("http://localhost/helpdesk/hulp-robot", _navManager.Uri);
    }

    [Fact]
    public void NavigateToVideos()
    {
        // Arrange
        var cut = RenderComponent<HelpdeskPage>();

        // Act
        var button = cut.FindAll("button").First(b => b.TextContent.Contains("Informatie-video's"));
        button.Click();

        // Assert
        Assert.Equal("http://localhost/helpdesk/videos", _navManager.Uri);
    }

    [Fact]
    public void NavigateToVoorlichting()
    {
        // Arrange
        var cut = RenderComponent<HelpdeskPage>();

        // Act
        var button = cut.FindAll("button").First(b => b.TextContent.Contains("Voorlichting"));
        button.Click();

        // Assert
        Assert.Equal("http://localhost/helpdesk/voorlichting", _navManager.Uri);
    }
}
