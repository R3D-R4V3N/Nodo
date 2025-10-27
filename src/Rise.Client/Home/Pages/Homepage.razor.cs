using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Assets;
using Rise.Shared.Chats;
using Rise.Shared.Users;
using System.Globalization;

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
        var currentAccountId = CurrentUser?.AccountId;
        var chatUserNames = chat?.Users?
            .Where(x => string.IsNullOrWhiteSpace(currentAccountId) || x.AccountId != currentAccountId)
            .Select(x => x.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        if (chatUserNames is { Count: > 0 })
        {
            return string.Join(", ", chatUserNames);
        }

        return $"Chat {chat?.ChatId}";
    }

    private string GetChatAvatar(ChatDto.GetChats chat)
    {
        var currentAccountId = CurrentUser?.AccountId;
        var participant = chat.Users?
            .FirstOrDefault(x => string.IsNullOrWhiteSpace(currentAccountId) || x.AccountId != currentAccountId);

        if (!string.IsNullOrWhiteSpace(participant?.AvatarUrl))
        {
            return participant!.AvatarUrl;
        }

        var fallbackKey = participant?.AccountId ?? chat.ChatId.ToString();
        return DefaultImages.GetProfile(fallbackKey);
    }

    private string GetParticipantsDescription(ChatDto.GetChats chat)
    {
        var currentAccountId = CurrentUser?.AccountId;
        var names = chat.Users?
            .Where(x => string.IsNullOrWhiteSpace(currentAccountId) || x.AccountId != currentAccountId)
            .Select(x => x.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList();

        return names is { Count: > 0 }
            ? string.Join(", ", names)
            : $"Chat {chat.ChatId}";
    }

    private string GetCurrentUserAvatar()
    {
        if (!string.IsNullOrWhiteSpace(CurrentUser?.AvatarUrl))
        {
            return CurrentUser.AvatarUrl;
        }

        return DefaultImages.GetProfile(CurrentUser?.AccountId);
    }

    private string GetLastMessagePreview(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        if (last is null || string.IsNullOrWhiteSpace(last.Content))
        {
            return "Nog geen berichten";
        }

        var senderLabel = last.User?.AccountId == CurrentUser?.AccountId
            ? "Jij"
            : string.IsNullOrWhiteSpace(last.User?.Name) ? string.Empty : last.User!.Name;

        var preview = last.Content.Trim();
        if (preview.Length > 70)
        {
            preview = string.Concat(preview.AsSpan(0, 70), "…");
        }

        return string.IsNullOrWhiteSpace(senderLabel)
            ? preview
            : $"{senderLabel}: {preview}";
    }

    private string GetLastActivityLabel(ChatDto.GetChats chat)
    {
        var timestamp = chat.LastMessage?.Timestamp;
        if (timestamp is null)
        {
            return "-";
        }

        var local = ToLocal(timestamp.Value);
        var today = DateTime.Today;

        if (local.Date == today)
        {
            return local.ToString("HH:mm");
        }

        if (local.Date == today.AddDays(-1))
        {
            return "Gisteren";
        }

        if (local.Date >= today.AddDays(-6))
        {
            return local.ToString("ddd", new CultureInfo("nl-NL"));
        }

        return local.ToString("dd MMM", new CultureInfo("nl-NL"));
    }

    private bool IsRecentlyActive(ChatDto.GetChats chat)
    {
        var timestamp = chat.LastMessage?.Timestamp;
        if (timestamp is null)
        {
            return false;
        }

        var local = ToLocal(timestamp.Value);
        return local >= DateTime.Now.AddHours(-6);
    }

    private void NavigateToProfile()
        => NavigationManager.NavigateTo("/profile");

    private void NavigateToFriends()
        => NavigationManager.NavigateTo("/friends");

    private static DateTime ToLocal(DateTime timestamp)
        => timestamp.Kind switch
        {
            DateTimeKind.Local => timestamp,
            DateTimeKind.Utc => timestamp.ToLocalTime(),
            _ => DateTime.SpecifyKind(timestamp, DateTimeKind.Utc).ToLocalTime()
        };
}
