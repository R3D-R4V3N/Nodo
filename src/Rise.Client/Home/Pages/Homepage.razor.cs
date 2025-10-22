using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Chats;
using System.Security.Claims;

namespace Rise.Client.Home.Pages;
public partial class Homepage
{
    private readonly List<ChatDto.GetChats> _chats = new();
    private List<ChatDto.GetChats> _filteredChats => string.IsNullOrWhiteSpace(_searchTerm)
        ? _chats
        : _chats.Where(c =>
                GetChatTitle(c).Contains(_searchTerm, StringComparison.OrdinalIgnoreCase) ||
                c.Messages.Any(m => m.Content.Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)))
            .ToList();

    private string? _currentAccountId;
    private string? _currentUserName;
    private string _greetingName = "gebruiker";
    private bool _isLoading = true;
    private string? _loadError;
    private string? _searchTerm;

    protected override async Task OnInitializedAsync()
    {
        var authState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            NavigationManager.NavigateTo("/login", true);
            return;
        }

        _currentAccountId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        _currentUserName = user.Identity?.Name;
        _greetingName = DetermineGreetingNameFromIdentity();

        var result = await ChatService.GetAllAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            _chats.Clear();
            _chats.AddRange(result.Value.Chats ?? Array.Empty<ChatDto.GetChats>());
            UpdateGreetingNameFromChats();
        }
        else
        {
            _loadError = result.Errors.FirstOrDefault() ?? "De chats konden niet geladen worden.";
        }

        _isLoading = false;
    }

    private void NavigateToChat(ChatDto.GetChats chat) => NavigationManager.NavigateTo($"/chat/{chat.ChatId}");

    private string GetChatTitle(ChatDto.GetChats chat)
    {
        var chatUserNames = chat?
            .Users
            .Where(x => x.AccountId != _currentAccountId)
            .Select(x => x.Name)
            .ToList() ?? [$"Chat {chat?.ChatId}"];

        return string.Join(", ", chatUserNames);
    }

    private static string GetLastMessagePreview(ChatDto.GetChats chat)
    {
        var last = chat.Messages.OrderBy(m => m.Timestamp).LastOrDefault();
        if (last is null || string.IsNullOrWhiteSpace(last.Content)) return "Nog geen berichten";
        var preview = last.Content.Trim();
        return preview.Length <= 80 ? preview : string.Concat(preview.AsSpan(0, 80), "…");
    }

    private static string GetLastActivity(ChatDto.GetChats chat)
    {
        var last = chat.Messages.OrderBy(m => m.Timestamp).LastOrDefault();
        return last?.Timestamp?.ToString("HH:mm") ?? "-";
    }

    private string DetermineGreetingNameFromIdentity()
    {
        if (!string.IsNullOrWhiteSpace(_currentUserName))
        {
            var at = _currentUserName.IndexOf('@');
            return at > 0 ? _currentUserName[..at] : _currentUserName;
        }
        return "gebruiker";
    }

    private void UpdateGreetingNameFromChats()
    {
        if (string.IsNullOrWhiteSpace(_currentAccountId)) return;
        var own = _chats.SelectMany(c => c.Messages)
                       .FirstOrDefault(m => string.Equals(m.User.AccountId, _currentAccountId, StringComparison.Ordinal));
        if (own is not null && !string.IsNullOrWhiteSpace(own.User.Name))
            _greetingName = own.User.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? _greetingName;
    }
}