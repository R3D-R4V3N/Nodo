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
    private int? _activeChatId;

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

            _hubConnection.On<MessageDto.Chat>("MessageCreated", message =>
                InvokeAsync(() => HandleIncomingMessage(message)));

            _hubConnection.Reconnected += _ =>
                InvokeAsync(async () =>
                {
                    await JoinAllChatsAsync();
                    StateHasChanged();
                });

            await _hubConnection.StartAsync();

            await JoinAllChatsAsync();

            // Vraag de huidige online users op
            var onlineNow = await _hubConnection.InvokeAsync<List<string>>("GetOnlineUsers");
            foreach (var id in onlineNow)
                _onlineUsers.Add(id);

            StateHasChanged();
        }
        catch (HttpRequestException)
        {
            // Offline status updates are optional; continue rendering cached chats without a notice.
        }
    }

    private Task JoinAllChatsAsync()
    {
        if (_hubConnection is null || _hubConnection.State != HubConnectionState.Connected)
        {
            return Task.CompletedTask;
        }

        var joinTasks = _chats
            .Select(chat => _hubConnection.SendAsync("JoinChat", chat.ChatId))
            .ToArray();

        return Task.WhenAll(joinTasks);
    }

    private void HandleIncomingMessage(MessageDto.Chat message)
    {
        var chat = _chats.FirstOrDefault(chat => chat.ChatId == message.ChatId);
        if (chat is null)
        {
            return;
        }

        var existingTimestamp = chat.LastMessage?.Timestamp;
        var incomingTimestamp = message.Timestamp;

        if (chat.LastMessage is null || existingTimestamp is null || incomingTimestamp is null || incomingTimestamp >= existingTimestamp)
        {
            chat.LastMessage = message;
        }

        var isActiveChat = _activeChatId.HasValue && _activeChatId.Value == message.ChatId;
        var isOwnMessage = string.Equals(message.User.AccountId, UserState.User?.AccountId, StringComparison.Ordinal);

        if (isActiveChat)
        {
            chat.UnreadCount = 0;
        }
        else if (!isOwnMessage)
        {
            chat.UnreadCount += 1;
        }

        _chats.Sort((a, b) => Nullable.Compare(b.LastMessage?.Timestamp, a.LastMessage?.Timestamp));

        StateHasChanged();
    }


    private void NavigateToChat(ChatDto.GetChats chat)
    {
        _activeChatId = chat.ChatId;
        chat.UnreadCount = 0;
        _chats.Sort((a, b) => Nullable.Compare(b.LastMessage?.Timestamp, a.LastMessage?.Timestamp));
        NavigationManager.NavigateTo($"/chat/{chat.ChatId}");
    }

    private static string FormatUnreadLabel(int unreadCount)
    {
        if (unreadCount <= 0)
        {
            return string.Empty;
        }

        return unreadCount > 99
            ? "99+"
            : unreadCount.ToString(CultureInfo.InvariantCulture);
    }

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