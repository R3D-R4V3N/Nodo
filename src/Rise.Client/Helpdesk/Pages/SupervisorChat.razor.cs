using Microsoft.AspNetCore.Components;
using Rise.Shared.Users;

namespace Rise.Client.Helpdesk.Pages;

public partial class SupervisorChat : ComponentBase
{
    private double _footerHeight = 200;
    private const double _footerPaddingBuffer = 24;
    
    private UserResponse.CurrentUser? user;
    public class ChatMessage
    {
        public string Content { get; set; } = "";
        public bool IsUser { get; set; }
        public string AvatarUrl { get; set; } = "";
    }

    private List<ChatMessage> Messages = new();
    private string _draft = string.Empty;

    private Task HandleUserMessage(string text)
    {
        if (!string.IsNullOrWhiteSpace(text))
        {
            Messages.Add(new ChatMessage
            {
                Content = text,
                IsUser = true,
                AvatarUrl = "/images/default-user.png"
            });

            _draft = ""; // invoerveld leegmaken
        }

        return Task.CompletedTask;
    }

    private Task ApplySuggestion(string text)
    {
        _draft = text; // zet de tekst klaar in het invoerveld
        StateHasChanged();
        return Task.CompletedTask;
    }
    private string GetMessageHostPaddingStyle()
    {
        var padding = Math.Max(0, Math.Ceiling(_footerHeight + _footerPaddingBuffer));
        return $"padding-bottom: {padding}px;";
    }
}