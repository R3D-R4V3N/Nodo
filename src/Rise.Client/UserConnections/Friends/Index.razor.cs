using Microsoft.AspNetCore.Components;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;
using System.Linq;

namespace Rise.Client.UserConnections.Friends;

public partial class Index
{
    private IEnumerable<UserConnectionDTO>? _connections;
    private List<UserConnectionDTO> _filteredConnections = [];
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
        ApplyFilter();
    }
    private void ApplyQuery()
    {
        // call db
        ApplyFilter();
    }
    private void ApplyFilter()
    {
        IEnumerable<UserConnectionDTO> q = _connections?.ToList() ?? [];

        q = _selectedTab switch
        {
            UserConnectionTypeDto.Friend => q.Where(x => x.State.Equals(UserConnectionTypeDto.Friend)).OrderBy(x => x.Name),
            UserConnectionTypeDto.Request => q.Where(x => 
                x.State.Equals(UserConnectionTypeDto.IncomingFriendRequest)
                || x.State.Equals(UserConnectionTypeDto.OutgoingFriendRequest)),
            _ => q,
        };
        
        // better to query db
        if (!string.IsNullOrWhiteSpace(_query))
            q = q.Where(f => f.Name.Contains(_query, StringComparison.OrdinalIgnoreCase));

        _filteredConnections = q.ToList();
        StateHasChanged();
    }
    private void OpenChat(UserConnectionDTO f) => Nav.NavigateTo("/chat"); // TODO: use chat id
    private void GoBack() => Nav.NavigateTo("/"); // TODO: maybe callback
    private void AcceptFriendRequest(UserConnectionDTO f)
    {
        //TODO: link websocket to every friend request method
        //f.State = UserConnectionTypeDto.Friend;
        ApplyFilter();
    }

    private void RejectFriendRequest(UserConnectionDTO f)
    {
        //_all.Remove(f);
        ApplyFilter();
    }
    private void CancelFriendRequest(UserConnectionDTO f)
    {
        //TODO: link websocket to every friend request method
        //f.State = UserConnectionTypeDto.Friend;
        ApplyFilter();
    }

    private void AddFriend(UserConnectionDTO f)
    {
        //f.State = UserConnectionTypeDto.Friend;
        ApplyFilter();
    }

    private void RemoveFriend(UserConnectionDTO f)
    {
        //_all.Remove(f);
        ApplyFilter();
    }
}
