using System.Globalization;
using System.Net.Http;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Shared.Assets;
using Rise.Shared.Chats;

namespace Rise.Client.Home.Pages;

public partial class Homepage : IDisposable
{
    [Inject] public UserState UserState { get; set; }
    private readonly List<ChatDto.GetChats> _chats = new();
    private List<ChatDto.GetChats> _filteredChats => string.IsNullOrWhiteSpace(_searchTerm)
        ? _chats
        : _chats.Where(c => GetChatTitle(c)
            .Contains(_searchTerm, StringComparison.OrdinalIgnoreCase)
        ).ToList();

    private bool _isLoading = true;
    private string? _loadError;
    private string? _searchTerm;

    [Inject]
    private IHubClientFactory HubClientFactory { get; set; } = null!;
    private IHubClient? _hubConnection;
    private readonly HashSet<string> _onlineUsers = new();

    protected override async Task OnInitializedAsync()
    {
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

        await InitializeHubAsync();

    }
    
    private async Task InitializeHubAsync()
    {
        try
        {
            _hubConnection = HubClientFactory.Create();

            // Wanneer iemand online of offline gaat
            _hubConnection.On<string, bool>("UserStatusChanged", (userId, isOnline) =>
            {
                if (isOnline)
                    _onlineUsers.Add(userId);
                else
                    _onlineUsers.Remove(userId);

                InvokeAsync(StateHasChanged); // UI updaten
            });

            _hubConnection.On<MessageDto.Chat>("MessageCreated", dto =>
            {
                _ = HandleIncomingMessageAsync(dto);
            });

            _hubConnection.Reconnected += _ => InvokeAsync(async () =>
            {
                await RefreshOnlineUsersAsync();
                await JoinChatGroupsAsync();
            });

            await _hubConnection.StartAsync();

            await RefreshOnlineUsersAsync();
            await JoinChatGroupsAsync();
        }
        catch (HttpRequestException)
        {
            // Offline status updates are optional; continue rendering cached chats without a notice.
        }
    }

    private async Task RefreshOnlineUsersAsync()
    {
        if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
        {
            return;
        }

        var onlineNow = await _hubConnection.InvokeAsync<List<string>>("GetOnlineUsers");
        _onlineUsers.Clear();
        foreach (var id in onlineNow)
            _onlineUsers.Add(id);

        StateHasChanged();
    }

    private async Task JoinChatGroupsAsync()
    {
        if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
        {
            return;
        }

        foreach (var chatId in _chats.Select(chat => chat.ChatId))
        {
            await _hubConnection.SendAsync("JoinChat", chatId);
        }
    }

    private Task HandleIncomingMessageAsync(MessageDto.Chat dto)
    {
        return InvokeAsync(() =>
        {
            var chat = _chats.FirstOrDefault(c => c.ChatId == dto.ChatId);
            if (chat is null)
            {
                return;
            }

            chat.LastMessage = dto;
            if (!string.Equals(dto.User.AccountId, UserState.User?.AccountId, StringComparison.Ordinal))
            {
                chat.UnreadCount++;
            }
            _chats.Remove(chat);
            _chats.Insert(0, chat);

            StateHasChanged();
        });
    }


    private void NavigateToChat(ChatDto.GetChats chat)
        => NavigationManager.NavigateTo($"/chat/{chat.ChatId}");

    private string GetChatTitle(ChatDto.GetChats chat)
    {
        var chatUserNames = chat?
            .Users
            .Where(x => x.Id != UserState.User!.Id)
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
            ? preview : 
            string.Concat(preview.AsSpan(0, 80), "�");
    }

    private static string GetLastActivity(ChatDto.GetChats chat)
    {
        var last = chat.LastMessage;
        return last?.Timestamp?.ToString("HH:mm") ?? "-";
    }

    private string DetermineGreetingNameFromIdentity()
    {
        return UserState.User!.Name;
    }

   private string GetAvatarUrl(ChatDto.GetChats chat)
   {
       // Zoek de eerste gebruiker die niet de huidige gebruiker is
       var otherUser = chat?
           .Users?
           .FirstOrDefault(u => u.Id != UserState.User.Id);

       return otherUser?.AvatarUrl ?? DefaultImages.Profile;
   }


    private static string GetAvatarKey(MessageDto.Chat dto)
    {
        if (!string.IsNullOrWhiteSpace(dto.User.AccountId))
            return dto.User.AccountId;

        if (dto.User.Id != 0)
            return dto.User.Id.ToString(CultureInfo.InvariantCulture);

        return dto.User.Name ?? string.Empty;
    }
    private bool IsGroupChat(ChatDto.GetChats chat)
    {
        return chat.Users.Count > 2;
    }

    private static string FormatUnreadLabel(int unreadCount)
    {
        if (unreadCount <= 0)
        {
            return string.Empty;
        }

        return unreadCount > 99 ? "99+" : unreadCount.ToString(CultureInfo.InvariantCulture);
    }

    private void NavigateToFriendProfile(ChatDto.GetChats chat)
    {
        var otherParticipant = chat.Users
            .Where(m => !string.Equals(m.AccountId, UserState.User.AccountId, StringComparison.Ordinal))
            .FirstOrDefault();

        if (otherParticipant is not null)
        {
            var accountId = otherParticipant.AccountId; // Dit is dus ApplicationUser.AccountId
            NavigationManager.NavigateTo($"/FriendProfilePage/{accountId}");
        }
    }

    public async void Dispose()
    {
        if (_hubConnection is not null)
            await _hubConnection.DisposeAsync();
    }
}