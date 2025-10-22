using Microsoft.AspNetCore.Components;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;

namespace Rise.Client.UserConnections.Pages.Friends;

public partial class Index
{
    private IEnumerable<UserConnectionDto.GetFriends>? _connections;
    private List<UserConnectionDto.GetFriends> _filteredConnections = [];
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

        var result = await ConnectionService.GetFriendIndexAsync(request);
        _connections = result.Value.Connections;
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
    private void ApplyFilter()
    {
        IEnumerable<UserConnectionDto.GetFriends> q = _connections?.ToList() ?? [];

        q = _selectedTab switch
        {
            UserConnectionTypeDto.Friend => q.Where(x => x.State == UserConnectionTypeDto.Friend).OrderBy(x => x.User.Name),
            UserConnectionTypeDto.Request => q.Where(x =>
                x.State == UserConnectionTypeDto.IncomingFriendRequest
                || x.State == UserConnectionTypeDto.OutgoingFriendRequest).OrderBy(x => x.User.Name),
            UserConnectionTypeDto.AddFriends => q.Where(x => x.State == UserConnectionTypeDto.AddFriends).OrderBy(x => x.User.Name),
            _ => q.OrderBy(x => x.User.Name),
        };
        
        // better to query db
        if (!string.IsNullOrWhiteSpace(_query))
            q = q.Where(f => f.User.Name.Contains(_query, StringComparison.OrdinalIgnoreCase));

        _filteredConnections = q.ToList();
        StateHasChanged();
    }
    private void OpenChat(UserConnectionDto.GetFriends f) => Nav.NavigateTo("/chat"); // TODO: use chat id
    private void GoBack() => Nav.NavigateTo("/"); // TODO: maybe callback
    private void AcceptFriendRequest(UserConnectionDto.GetFriends f)
    {
        //TODO: link websocket to every friend request method
        //f.State = UserConnectionTypeDto.Friend;
        ApplyFilter();
    }

    private void RejectFriendRequest(UserConnectionDto.GetFriends f)
    {
        //_all.Remove(f);
        ApplyFilter();
    }
    private void CancelFriendRequest(UserConnectionDto.GetFriends f)
    {
        //TODO: link websocket to every friend request method
        //f.State = UserConnectionTypeDto.Friend;
        ApplyFilter();
    }

    private void AddFriend(UserConnectionDto.GetFriends f)
    {
        //f.State = UserConnectionTypeDto.Friend;
        ApplyFilter();
    }

    private void RemoveFriend(UserConnectionDto.GetFriends f)
    {
        //_all.Remove(f);
        ApplyFilter();
    }
}
