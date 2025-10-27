using System;
using System.Linq;
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

    private static string GetLastMessagePreview(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        if (last is null || string.IsNullOrWhiteSpace(last.Content))
            return "Nog geen berichten";

        var preview = last.Content;

        return preview.Length <= 80
            ? preview
            : $"{preview.AsSpan(0, 80)}...";
    }

    private static string GetLastActivity(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        return last?.Timestamp?.ToString("HH:mm") ?? "-";
    }

    private string GetGreeting()
    {
        var hour = DateTime.Now.Hour;
        return hour switch
        {
            < 12 => "Goeiemorgen",
            < 18 => "Goeiemiddag",
            _ => "Goeieavond"
        };
    }

    private ChatDto.GetChats? GetMostRecentChat()
        => _chats
            .OrderByDescending(c => c.LastMessage?.Timestamp ?? DateTime.MinValue)
            .FirstOrDefault();

    private string GetChatInitials(ChatDto.GetChats chat)
    {
        var title = GetChatTitle(chat);
        if (string.IsNullOrWhiteSpace(title))
        {
            return "N";
        }

        var parts = title
            .Split(new[] { ' ', ',', '|' }, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (parts.Length == 0)
        {
            return "N";
        }

        if (parts.Length == 1)
        {
            var snippet = new string(parts[0].Take(2).ToArray());
            return snippet.ToUpperInvariant();
        }

        return new string(new[]
        {
            char.ToUpperInvariant(parts[0][0]),
            char.ToUpperInvariant(parts[1][0])
        });
    }
}
