using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Chats;
using Rise.Shared.Users;

namespace Rise.Client.Home.Pages;
public partial class Homepage
{
    [CascadingParameter] public UserDto.CurrentUser? CurrentUser { get; set; }
    private readonly List<ChatDto.GetChats> _chats = new();
    private List<ChatDto.GetChats> _filteredChats => string.IsNullOrWhiteSpace(_searchTerm)
        ? _chats
        : _chats.Where(c => GetChatTitle(c)
            .Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();

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

        var result = await ChatService.GetAllAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            _chats.Clear();
            _chats.AddRange(result.Value.Chats ?? []);
        }
        else
        {
            _loadError = result.Errors.FirstOrDefault() ?? "De chats konden niet geladen worden.";
        }

        _isLoading = false;
    }

    private void NavigateToChat(ChatDto.GetChats chat) 
        => NavigationManager.NavigateTo($"/chat/{chat.ChatId}");

    private string GetChatTitle(ChatDto.GetChats chat)
    {
        var chatUserNames = chat?
            .Users
            .Where(x => x.AccountId != CurrentUser!.AccountId)
            .Select(x => x.Name)
            .ToList() ?? [$"Chat {chat?.ChatId}"];

        return string.Join(", ", chatUserNames);
    }

    private string GetChatInitial(ChatDto.GetChats chat)
    {
        var title = GetChatTitle(chat).Trim();
        if (string.IsNullOrEmpty(title))
        {
            return "?";
        }

        var first = title[0];
        return char.IsLetterOrDigit(first)
            ? first.ToString().ToUpperInvariant()
            : first.ToString();
    }

    private static string GetLastMessagePreview(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        if (last is null || string.IsNullOrWhiteSpace(last.Content))
            return "Nog geen berichten";

        var preview = last.Content;

        const int maxLength = 80;
        return preview.Length <= maxLength
            ? preview
            : string.Concat(preview.AsSpan(0, maxLength), "…");
    }

    private static string GetLastActivity(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        return last?.Timestamp?.ToString("HH:mm") ?? "-";
    }
}