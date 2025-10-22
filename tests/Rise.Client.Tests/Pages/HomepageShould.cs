using System.Security.Claims;
using AngleSharp.Dom;
using Ardalis.Result;
using Bunit;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Components.Authorization;
using NSubstitute;
using Rise.Client.Pages;
using Rise.Shared.Chats;
using Rise.Client.Chats;
using Xunit.Abstractions;
using Rise.Client.Home.Pages;

public class HomepageShould : TestContext
{
    public HomepageShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
    }

    [Fact]
    public async Task ShowNavbarWhenLoaded()
    {
        // Arrange
        var fakeChats = new List<ChatDto.GetChats>(); // geen chats nodig voor deze test

        var mockChatService = Substitute.For<IChatService>();
        mockChatService.GetAllAsync()
            .Returns(Result.Success(new ChatResponse.GetChats { Chats = fakeChats }));

        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "kyandro@nodo.chat")
            },
            "TestAuth"));

        var mockAuth = Substitute.For<AuthenticationStateProvider>();
        mockAuth.GetAuthenticationStateAsync()
            .Returns(new AuthenticationState(fakeUser));

        Services.AddSingleton<IChatService>(mockChatService);
        Services.AddSingleton<AuthenticationStateProvider>(mockAuth);

        // Act
        var cut = RenderComponent<Homepage>();

        // Wacht tot de component klaar is met laden
        cut.WaitForAssertion(() =>
            Assert.DoesNotContain("Chats laden…", cut.Markup));

        // Assert
        var header = cut.Find("header");
        Assert.NotNull(header);
        Assert.Contains("Goeiemorgen", header.TextContent);
        var inputfield = header.QuerySelector("input");
        Assert.NotNull(inputfield);
        
        var chats = cut.Find("section");
        Assert.NotNull(chats);
    }
    
    [Fact]
    public void ShowChatsWhenLoaded()
    {
        // Arrange
        var mockChatService = Substitute.For<IChatService>();
        mockChatService.GetAllAsync().Returns(Task.FromResult(Result.Success(new ChatResponse.GetChats
        {
            Chats = new List<ChatDto.GetChats>
            {
                new ChatDto.GetChats
                {
                    ChatId = 1,
                    Messages = new List<MessageDto.Chat>
                    {
                        new MessageDto.Chat
                        {
                            Content = "Hallo, testbericht",
                            Timestamp = DateTime.UtcNow,
                            User = new Rise.Shared.Users.UserDto.Message()
                            {
                                Id = 1,
                                AccountId = "1",
                                AvatarUrl = "",
                                Name = "Kyandro voet"
                            }
                        }
                    }
                }
            }
        })));

        Services.AddSingleton(mockChatService);

        // Voeg fake authenticated user toe
        var fakeUser = new ClaimsPrincipal(new ClaimsIdentity(
            new[]
            {
                new Claim(ClaimTypes.NameIdentifier, "1"),
                new Claim(ClaimTypes.Name, "kyandro@nodo.chat")
            },
            "TestAuth"));

        var mockAuth = Substitute.For<AuthenticationStateProvider>();
        mockAuth.GetAuthenticationStateAsync().Returns(Task.FromResult(new AuthenticationState(fakeUser)));
        Services.AddSingleton<AuthenticationStateProvider>(mockAuth);

        // Act
        var cut = RenderComponent<Homepage>();

        // Wait for component to finish loading
        cut.WaitForAssertion(() => Assert.DoesNotContain("Chats laden…", cut.Markup));

        // Assert
        var chatItems = cut.FindAll("ul li");
        Assert.Single(chatItems); // er is 1 chat
        Assert.Contains("Hallo, testbericht", chatItems[0].TextContent);
        Assert.Contains("Kyandro", chatItems[0].TextContent);
    }
    
}