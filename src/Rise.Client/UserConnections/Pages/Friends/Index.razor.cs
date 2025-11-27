using System;
using System.Threading;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using Rise.Client.State;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;

namespace Rise.Client.UserConnections.Pages.Friends;

public partial class Index : IAsyncDisposable
{
    private readonly SemaphoreSlim _hubConnectionLock = new(1, 1);
    private readonly SemaphoreSlim _dataLock = new(1, 1);
    private IEnumerable<UserConnectionDto.Get> _friends = [];
    private IEnumerable<UserConnectionDto.Get> _requests = [];
    private IEnumerable<UserConnectionDto.Get> _suggestions = [];
    private List<UserConnectionDto.Get> _filteredConnections = [];
    private UserConnectionTypeDto _selectedTab = UserConnectionTypeDto.Friend;
    private HubConnection? _hubConnection;
    private string? _query;
    private string? _connectionError;
    private bool _joinedRealtimeGroup;

    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] public required IUserConnectionService ConnectionService { get; set; }
    [Inject] public required UserState UserState { get; set; }

    private string? Query
    {
        get => _query;
        set
        {
            if (_query == value)
            {
                return;
            }

            _query = value;
            _ = ApplyFilterAsync();
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await ApplyFilterAsync();
        await EnsureHubConnectionAsync();
    }

    private async Task ChangeTab(UserConnectionTypeDto tab)
    {
        _selectedTab = tab;
        _query = string.Empty;
        await ApplyFilterAsync();
    }

    private Task OpenChat(UserConnectionDto.Get f)
    {
        Nav.NavigateTo($"/chat/{f.User.ChatId}"); 
        return Task.CompletedTask;
    }

    private Task GoBack()
    {
        Nav.NavigateTo("/");
        return Task.CompletedTask;
    }

    private async Task AcceptFriendRequest(UserConnectionDto.Get f)
    {
        await ConnectionService.AcceptFriendRequestAsync(f.User.AccountId);
        await ApplyFilterAsync();
    }

    private async Task RejectFriendRequest(UserConnectionDto.Get f)
    {
        await ConnectionService.RejectFriendRequestAsync(f.User.AccountId);
        await ApplyFilterAsync();
    }

    private async Task CancelFriendRequest(UserConnectionDto.Get f)
    {
        await ConnectionService.CancelFriendRequest(f.User.AccountId);
        await ApplyFilterAsync();
    }

    private async Task AddFriend(UserConnectionDto.Get f)
    {
        await ConnectionService.SendFriendRequestAsync(f.User.AccountId);
        await ApplyFilterAsync();
    }

    private async Task RemoveFriend(UserConnectionDto.Get f)
    {
        await ConnectionService.RemoveFriendAsync(f.User.AccountId);
        await ApplyFilterAsync();
    }

    private async Task ReloadDataAsync()
    {
        var request = new QueryRequest.SkipTake
        {
            Skip = 0,
            Take = int.MaxValue,
        };

        var friendsTask = ConnectionService.GetFriendsAsync(request);
        var requestsTask = ConnectionService.GetFriendRequestsAsync(request);
        var suggestionsTask = ConnectionService.GetSuggestedFriendsAsync(request);

        await Task.WhenAll(friendsTask, requestsTask, suggestionsTask);

        _friends = friendsTask.Result.Value.Connections;
        _requests = requestsTask.Result.Value.Connections;
        _suggestions = suggestionsTask.Result.Value.Users;
    }

    private async Task ApplyFilterAsync()
    {
        await _dataLock.WaitAsync();
        try
        {
            await ReloadDataAsync();

            IEnumerable<UserConnectionDto.Get> query = _selectedTab switch
            {
                UserConnectionTypeDto.Friend => _friends,
                UserConnectionTypeDto.Request => _requests,
                UserConnectionTypeDto.AddFriends => _suggestions,
                _ => throw new NotImplementedException($"{_selectedTab} not implemented."),
            };

            if (!string.IsNullOrWhiteSpace(_query))
            {
                query = query.Where(f =>
                    f.User.Name.Contains(_query, StringComparison.OrdinalIgnoreCase));
            }

            _filteredConnections = query.ToList();
        }
        finally
        {
            _dataLock.Release();
        }

        await InvokeAsync(StateHasChanged);
    }

    private async Task EnsureHubConnectionAsync()
    {
        if (UserState.User?.AccountId is null)
        {
            return;
        }

        await _hubConnectionLock.WaitAsync();
        try
        {
            if (_hubConnection is null)
            {
                _hubConnection = new HubConnectionBuilder()
                    .WithUrl(Nav.ToAbsoluteUri("/connectionsHub"))
                    .WithAutomaticReconnect()
                    .Build();

                _hubConnection.On<string>("FriendConnectionsChanged", accountId =>
                    InvokeAsync(() => HandleFriendConnectionsChangedAsync(accountId)));

                _hubConnection.Reconnecting += _ =>
                {
                    _connectionError = "Realtime verbinding wordt hersteld…";
                    return InvokeAsync(StateHasChanged);
                };

                _hubConnection.Reconnected += _ =>
                {
                    _joinedRealtimeGroup = false;
                    _connectionError = null;
                    return InvokeAsync(JoinRealtimeGroupAfterReconnectAsync);
                };

                _hubConnection.Closed += _ =>
                {
                    _joinedRealtimeGroup = false;
                    _connectionError = "Realtime verbinding werd verbroken. Vernieuw de pagina om opnieuw te verbinden.";
                    return InvokeAsync(StateHasChanged);
                };
            }

            if (_hubConnection.State == HubConnectionState.Disconnected)
            {
                await _hubConnection.StartAsync();
            }

            if (_hubConnection.State == HubConnectionState.Connected)
            {
                await JoinRealtimeGroupAsync();
            }
        }
        catch (Exception ex)
        {
            _connectionError = $"Realtime verbinding mislukt: {ex.Message}";
            await InvokeAsync(StateHasChanged);
        }
        finally
        {
            _hubConnectionLock.Release();
        }
    }

    private async Task JoinRealtimeGroupAfterReconnectAsync()
    {
        try
        {
            await JoinRealtimeGroupAsync();
        }
        finally
        {
            StateHasChanged();
        }
    }

    private async Task JoinRealtimeGroupAsync()
    {
        if (_hubConnection is null || UserState.User?.AccountId is null || _joinedRealtimeGroup)
        {
            return;
        }

        await _hubConnection.SendAsync("JoinConnections");
        _joinedRealtimeGroup = true;
    }

    private async Task HandleFriendConnectionsChangedAsync(string accountId)
    {
        if (UserState.User?.AccountId is null)
        {
            return;
        }

        if (!string.Equals(UserState.User.AccountId, accountId, StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        await ApplyFilterAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (_hubConnection is not null)
        {
            try
            {
                if (_hubConnection.State == HubConnectionState.Connected)
                {
                    await _hubConnection.SendAsync("LeaveConnections");
                }
            }
            catch
            {
                // Ignore best-effort leave errors.
            }

            await _hubConnection.DisposeAsync();
        }

        _hubConnectionLock.Dispose();
        _dataLock.Dispose();
    }
}
