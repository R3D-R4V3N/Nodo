using System.Threading;
using Microsoft.AspNetCore.SignalR;
using Rise.Server.Hubs;
using Rise.Services.UserConnections;

namespace Rise.Server.RealTime;

public class SignalRUserConnectionNotificationDispatcher(IHubContext<UserConnectionHub> hubContext)
    : IUserConnectionNotificationDispatcher
{
    private readonly IHubContext<UserConnectionHub> _hubContext = hubContext;

    public Task NotifyFriendConnectionsChangedAsync(string accountId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Task.CompletedTask;
        }

        return _hubContext
            .Clients
            .Group(UserConnectionHub.GetGroupName(accountId))
            .SendAsync("FriendConnectionsChanged", accountId, cancellationToken);
    }
}
