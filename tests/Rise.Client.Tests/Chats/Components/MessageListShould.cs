using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using NSubstitute;
using Rise.Client.Chats.Components;
using Rise.Client.State;
using Rise.Shared.Chats;
using Rise.Shared.Users;
using Xunit.Abstractions;

namespace Rise.Client.Tests.Chats.Components;

public class MessageListShould : TestContext
{
    public MessageListShould(ITestOutputHelper outputHelper)
    {
        Services.AddXunitLogger(outputHelper);
        JSInterop.Mode = JSRuntimeMode.Loose; 
    }

    [Fact]
    public void RenderMessages()
    {
        // Arrange
        var user1 = new UserDto.Message { Id = 1, Name = "User 1", AvatarUrl = "u1.png" };
        var user2 = new UserDto.Message { Id = 2, Name = "User 2", AvatarUrl = "u2.png" };
        var currentUser = new UserDto.CurrentUser { Id = 1, AccountId = "1", FirstName = "User", LastName = "One" };

        var userState = new UserState { User = currentUser };
        Services.AddSingleton(userState);

        var messages = new List<MessageDto.Chat>
        {
            new() { Id = 1, Content = "Hello", User = user1, Timestamp = DateTime.Now.AddMinutes(-10) },
            new() { Id = 2, Content = "Hi there", User = user2, Timestamp = DateTime.Now.AddMinutes(-5) }
        };

        // Act
        var cut = RenderComponent<MessageList>(parameters => parameters
            .Add(p => p.Messages, messages)
            .Add(p => p.TimestampText, "Today")
        );

        // Assert
        Assert.Contains("Hello", cut.Markup);
        Assert.Contains("Hi there", cut.Markup);
        
        // Check grouping logic: User 2's name should appear
        Assert.Contains("User 2", cut.Markup);
        // User 1 is current user, usually outgoing messages don't show name in many UIs, 
        // but let's check the logic in MessageList.razor: 
        // var showSender = !isOutgoing && !string.IsNullOrWhiteSpace(message.User.Name) && !isContinuation;
        // User 1 is outgoing (Id=1 matches current user Id=1). So Name should NOT be shown.
        Assert.DoesNotContain("User 1", cut.Markup);
    }

    [Fact]
    public void GroupConsecutiveMessagesFromSameUser()
    {
        // Arrange
        var user2 = new UserDto.Message { Id = 2, Name = "User 2", AvatarUrl = "u2.png" };
        var currentUser = new UserDto.CurrentUser { Id = 1, AccountId = "1", FirstName = "User", LastName = "One" };

        var userState = new UserState { User = currentUser };
        Services.AddSingleton(userState);

        var messages = new List<MessageDto.Chat>
        {
            new() { Id = 2, Content = "First message", User = user2, Timestamp = DateTime.Now.AddMinutes(-5) },
            new() { Id = 3, Content = "Second message", User = user2, Timestamp = DateTime.Now.AddMinutes(-4) } // Within window
        };

        // Act
        var cut = RenderComponent<MessageList>(parameters => parameters
            .Add(p => p.Messages, messages)
        );

        // Assert
        // The first message should show the sender name
        // The second message should NOT show the sender name (grouped)
        // We look for the text. 
        // Note: Assert.Contains checks if string is in markup. 
        // We expect "User 2" to appear AT LEAST once.
        // To be strict, we might count occurrences or look at specific elements.
        // For now, let's verify "User 2" is present.
        Assert.Contains("User 2", cut.Markup);
        
        // To verify grouping, we can check if the second message wrapper has 'space-y-0.5' (continuation) vs 'space-y-1'
        // Logic: var isContinuation = ShouldGroupWithPrevious(message, previous);
        // class="@(isContinuation ? "space-y-0.5" : "space-y-1")"
        // We expect one "space-y-1" (first msg) and one "space-y-0.5" (second msg)
        
        var continuations = cut.FindAll(".space-y-0\\.5"); // Escaped . for CSS selector
        Assert.Single(continuations);
    }
}
