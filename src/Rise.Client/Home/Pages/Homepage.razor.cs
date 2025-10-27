using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Assets;
using Rise.Shared.Chats;
using Rise.Shared.Users;

namespace Rise.Client.Home.Pages;

public partial class Homepage
{
    private static readonly CultureInfo DutchCulture = new("nl-NL");

    private static readonly string[] AccentPalette =
    [
        "#DCFCE7",
        "#E0F2FE",
        "#FDE68A",
        "#FCE7F3",
        "#F5D0FE",
        "#FFE4E6",
        "#E9D5FF"
    ];

    private static readonly int[] LoadingPlaceholders = { 0, 1, 2, 3, 4, 5 };

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

        await LoadChatsAsync();
    }

    private async Task LoadChatsAsync()
    {
        var result = await ChatService.GetAllAsync();
        if (result.IsSuccess && result.Value is not null)
        {
            _chats.Clear();
            _chats.AddRange(result.Value.Chats ?? []);
            _loadError = null;
        }
        else
        {
            _loadError = result.Errors.FirstOrDefault() ?? "De chats konden niet geladen worden.";
        }

        _isLoading = false;
    }

    private async Task ReloadChats()
    {
        _isLoading = true;
        _loadError = null;
        await LoadChatsAsync();
    }

    private void ClearSearch() => _searchTerm = string.Empty;

    private void NavigateToFriends() => NavigationManager.NavigateTo("/friends");

    private void NavigateToChat(ChatDto.GetChats chat)
        => NavigationManager.NavigateTo($"/chat/{chat.ChatId}");

    private string GetChatTitle(ChatDto.GetChats chat)
    {
        var chatUserNames = chat?
            .Users
            .Where(x => CurrentUser is null || x.AccountId != CurrentUser.AccountId)
            .Select(x => x.Name)
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .ToList() ?? [$"Chat {chat?.ChatId}"];

        return string.Join(", ", chatUserNames);
    }

    private string GetChatAvatar(ChatDto.GetChats chat)
    {
        if (chat?.Users is null || chat.Users.Count == 0)
        {
            return DefaultImages.GetProfile(chat?.ChatId.ToString());
        }

        var candidate = chat.Users
            .FirstOrDefault(u => CurrentUser is null || u.AccountId != CurrentUser.AccountId)
            ?? chat.Users.First();

        if (!string.IsNullOrWhiteSpace(candidate.AvatarUrl))
        {
            return candidate.AvatarUrl;
        }

        return DefaultImages.GetProfile(candidate.AccountId);
    }

    private string GetGreetingName()
    {
        if (string.IsNullOrWhiteSpace(CurrentUser?.Name))
        {
            return "!";
        }

        var parts = CurrentUser!.Name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length > 0 ? parts[0] : CurrentUser.Name;
    }

    private string GetCurrentUserAvatar()
    {
        if (CurrentUser is null)
        {
            return DefaultImages.Profile;
        }

        return string.IsNullOrWhiteSpace(CurrentUser.AvatarUrl)
            ? DefaultImages.GetProfile(CurrentUser.AccountId)
            : CurrentUser.AvatarUrl;
    }

    private string GetChatStatisticLabel()
    {
        if (_isLoading)
        {
            return "Gesprekken worden geladen…";
        }

        if (!string.IsNullOrWhiteSpace(_loadError))
        {
            return "Er is een fout opgetreden tijdens het laden.";
        }

        if (_filteredChats.Count == 0)
        {
            return _chats.Count == 0
                ? "Nog geen gesprekken gestart."
                : "Geen gesprekken die passen bij je zoekopdracht.";
        }

        return _filteredChats.Count == 1
            ? "1 gesprek klaar om te hervatten."
            : $"{_filteredChats.Count} gesprekken klaar om te hervatten.";
    }

    private string GetLatestActivitySummary()
    {
        if (_isLoading || _chats.Count == 0)
        {
            return "Nog geen activiteit zichtbaar.";
        }

        var latest = _chats
            .Select(c => c.LastMessage?.Timestamp)
            .Where(t => t.HasValue)
            .Select(t => t!.Value)
            .OrderByDescending(t => t)
            .FirstOrDefault();

        if (latest == default)
        {
            return "Nog geen berichten verstuurd.";
        }

        return $"Laatste activiteit {FormatRelativeTime(latest)}";
    }

    private string GetChatStatusMessage()
    {
        if (_isLoading)
        {
            return "We halen je gesprekken op…";
        }

        if (!string.IsNullOrWhiteSpace(_loadError))
        {
            return "Kon gesprekken niet laden.";
        }

        if (_filteredChats.Count == 0)
        {
            return "Geen gesprekken gevonden.";
        }

        return _filteredChats.Count == 1
            ? "1 gesprek gevonden"
            : $"{_filteredChats.Count} gesprekken gevonden";
    }

    private string GetParticipantsPreview(ChatDto.GetChats chat)
    {
        if (chat?.Users is null || chat.Users.Count == 0)
        {
            return "Geen deelnemers";
        }

        var names = chat.Users
            .Where(u => CurrentUser is null || u.AccountId != CurrentUser.AccountId)
            .Select(u => u.Name)
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .ToList();

        if (names.Count == 0)
        {
            return CurrentUser is null ? "Gesprek" : "Met jezelf";
        }

        if (names.Count == 1)
        {
            return names[0];
        }

        if (names.Count == 2)
        {
            return $"{names[0]} en {names[1]}";
        }

        return $"{names[0]}, {names[1]} +{names.Count - 2}";
    }

    private static bool HasAudioMessage(ChatDto.GetChats chat)
        => !string.IsNullOrWhiteSpace(chat.LastMessage?.AudioDataUrl);

    private string GetLastMessagePreview(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        if (last is null)
        {
            return "Nog geen berichten. Start het gesprek!";
        }

        var sender = GetLastMessageSender(chat);

        if (!string.IsNullOrWhiteSpace(last.AudioDataUrl))
        {
            return string.IsNullOrWhiteSpace(sender)
                ? "Spraakbericht ontvangen."
                : $"{sender} stuurde een spraakbericht.";
        }

        var content = string.IsNullOrWhiteSpace(last.Content)
            ? "Nieuw bericht"
            : last.Content.Trim();

        if (content.Length > 110)
        {
            content = string.Concat(content.AsSpan(0, 107), "…");
        }

        return string.IsNullOrWhiteSpace(sender) ? content : $"{sender}: {content}";
    }

    private string GetLastActivity(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        if (last?.Timestamp is null)
        {
            return "geen activiteit";
        }

        return FormatRelativeTime(last.Timestamp.Value);
    }

    private string? GetSearchBadgeText()
    {
        if (string.IsNullOrWhiteSpace(_searchTerm))
        {
            return null;
        }

        return $"Zoekterm: \"{_searchTerm}\"";
    }

    private string GetEmptyStateDescription()
    {
        if (_chats.Count == 0 && string.IsNullOrWhiteSpace(_searchTerm))
        {
            return "Je hebt nog geen gesprekken. Voeg vrienden toe en begin een nieuw gesprek.";
        }

        if (_chats.Count == 0)
        {
            return "Je hebt nog geen gesprekken om te doorzoeken. Start een gesprek en probeer het opnieuw.";
        }

        return string.IsNullOrWhiteSpace(_searchTerm)
            ? "Er zijn nog geen chats beschikbaar."
            : $"We vonden geen resultaten voor \"{_searchTerm}\". Probeer een andere naam of verwijder je filter.";
    }

    private string GetLastMessageSender(ChatDto.GetChats chat)
    {
        var user = chat.LastMessage?.User;
        if (user is null)
        {
            return string.Empty;
        }

        if (CurrentUser is not null && string.Equals(user.AccountId, CurrentUser.AccountId, StringComparison.Ordinal))
        {
            return "Jij";
        }

        return user.Name;
    }

    private string GetAccentStyle(ChatDto.GetChats chat)
        => $"--accent-color: {GetChatAccentColor(chat)}";

    private string GetChatAccentColor(ChatDto.GetChats chat)
    {
        var key = chat?.ChatId.ToString() ?? string.Empty;
        var hash = 0;

        foreach (var character in key)
        {
            hash = unchecked((hash * 31) + char.ToLowerInvariant(character));
        }

        var index = (int)((uint)hash % (uint)AccentPalette.Length);
        return AccentPalette[index];
    }

    private static string FormatRelativeTime(DateTime timestamp)
    {
        var local = NormalizeToLocal(timestamp);
        var now = DateTime.Now;
        var difference = now - local;

        if (difference < TimeSpan.FromMinutes(1))
        {
            return "zojuist";
        }

        if (difference < TimeSpan.FromHours(1))
        {
            var minutes = Math.Max(1, (int)Math.Round(difference.TotalMinutes));
            return $"{minutes} min geleden";
        }

        if (local.Date == now.Date)
        {
            return $"om {local:HH:mm}";
        }

        if (local.Date == now.AddDays(-1).Date)
        {
            return "gisteren";
        }

        if (difference < TimeSpan.FromDays(7))
        {
            return local.ToString("dddd", DutchCulture);
        }

        if (local.Year == now.Year)
        {
            return local.ToString("d MMM", DutchCulture);
        }

        return local.ToString("d MMM yyyy", DutchCulture);
    }

    private static DateTime NormalizeToLocal(DateTime timestamp)
    {
        var kind = timestamp.Kind == DateTimeKind.Unspecified ? DateTimeKind.Utc : timestamp.Kind;
        var normalized = DateTime.SpecifyKind(timestamp, kind);
        return kind == DateTimeKind.Utc ? normalized.ToLocalTime() : normalized;
    }
}
