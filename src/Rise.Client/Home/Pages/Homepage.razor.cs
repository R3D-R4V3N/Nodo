using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Rise.Client.Offline;
using Rise.Client.RealTime;
using Rise.Client.State;
using Rise.Shared.Assets;
using Rise.Shared.Chats;

namespace Rise.Client.Home.Pages;

public partial class Homepage : IDisposable
{
    [Inject] public UserState UserState { get; set; }
    [Inject] public SessionCacheService SessionCacheService { get; set; } = null!;
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
        try
        {
            var cachedChats = await SessionCacheService.GetCachedChatsAsync();
            if (cachedChats.Count > 0)
            {
                _chats.Clear();
                _chats.AddRange(cachedChats);
                _isLoading = false;
            }
        }
        catch
        {
            // If cache fails, we continue with the live request below.
        }

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

        await InitializeHubAsync();

    }
    
    private async Task InitializeHubAsync()
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

        await _hubConnection.StartAsync();

        // Vraag de huidige online users op
        var onlineNow = await _hubConnection.InvokeAsync<List<string>>("GetOnlineUsers");
        foreach (var id in onlineNow)
            _onlineUsers.Add(id);

        StateHasChanged();
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