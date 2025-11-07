using Microsoft.AspNetCore.Components;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;

namespace Rise.Client.UserConnections.Pages.Friends;

public partial class Index
{
    private IEnumerable<UserConnectionDto.Get> _connections = [];
    private IEnumerable<UserConnectionDto.Get> _suggestions = [];
    private List<UserConnectionDto.Get> _filteredConnections = [];
    private UserConnectionTypeDto _selectedTab = UserConnectionTypeDto.Friend;
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] public required IUserConnectionService ConnectionService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var request = new QueryRequest.SkipTake
        {
            Skip = 0,
            Take = int.MaxValue,
        };

        var getFriendsResult = await ConnectionService.GetFriendIndexAsync(request);
        var getSuggestionResult = await ConnectionService.GetSuggestedFriendsAsync(request);

        _connections = getFriendsResult.Value.Connections;
        _suggestions = getSuggestionResult.Value.Users;
        ApplyFilter();
    }

    private void ChangeTab(UserConnectionTypeDto tab)
    {
        _selectedTab = tab;
        _query = string.Empty;
        ApplyFilter();
    }
    private void ApplyQuery()
    {
        // call db
        ApplyFilter();
    }
    private async void ApplyFilter()
    {
        await ReloadDataAsync();
        var q = _selectedTab switch
        {
            UserConnectionTypeDto.Friend => _connections
                .Where(x => x.State == UserConnectionTypeDto.Friend)
                .OrderBy(x => x.User.Name),
            UserConnectionTypeDto.Request => _connections
                .Where(x => x.State == UserConnectionTypeDto.IncomingFriendRequest
                || x.State == UserConnectionTypeDto.OutgoingFriendRequest)
                .OrderBy(x => x.User.Name),
            UserConnectionTypeDto.AddFriends => _suggestions!,

            _ => throw new NotImplementedException($"{_selectedTab} not impl"),
        };
        
        // better to query db
        if (!string.IsNullOrWhiteSpace(_query))
            q = q.Where(f => f.User.Name.Contains(_query, StringComparison.OrdinalIgnoreCase));

        _filteredConnections = q.ToList();
        StateHasChanged();
    }
    private void OpenChat(UserConnectionDto.Get f) => Nav.NavigateTo("/chat"); // TODO: use chat id
    private void GoBack() => Nav.NavigateTo("/"); // TODO: maybe callback
    private async void AcceptFriendRequest(UserConnectionDto.Get f)
    {
        ConnectionService.AcceptFriendRequestAsync( f.User.AccountId);
        ApplyFilter();
    }

    private void RejectFriendRequest(UserConnectionDto.Get f)
    {
        //_all.Remove(f);
        ConnectionService.RejectFriendRequestAsync(f.User.AccountId, CancellationToken.None);
        ApplyFilter();
    }
    private async void CancelFriendRequest(UserConnectionDto.Get f)
    {
        //TODO: link websocket to every friend request method
        ConnectionService.CancelFriendRequest(f.User.AccountId, CancellationToken.None);
        ApplyFilter();
    }

    private async void AddFriend(UserConnectionDto.Get f)
    {
        //f.State = UserConnectionTypeDto.Friend;
        ConnectionService.SendFriendRequestAsync(f.User.AccountId);
        ApplyFilter();
    }

    private void RemoveFriend(UserConnectionDto.Get f)
    {
        //_all.Remove(f);
        ApplyFilter();
    }
    
    private async Task ReloadDataAsync()
    {
        var request = new QueryRequest.SkipTake
        {
            Skip = 0,
            Take = int.MaxValue,
        };

        var getFriendsResult = await ConnectionService.GetFriendIndexAsync(request);
        var getSuggestionResult = await ConnectionService.GetSuggestedFriendsAsync(request);

        _connections = getFriendsResult.Value.Connections;
        _suggestions = getSuggestionResult.Value.Users;

        //ApplyFilter();
    }
}
