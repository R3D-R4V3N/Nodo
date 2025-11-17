using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Rise.Server.Hubs;

[Authorize]
public class UserConnectionHub : Hub
{
    public static string GetGroupName(string accountId) => $"user-connections-{accountId}";

    public Task JoinConnections()
    {
        var accountId = GetAccountId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            throw new HubException("Gebruikersaccount kon niet bepaald worden.");
        }

        return Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(accountId));
    }

    public Task LeaveConnections()
    {
        var accountId = GetAccountId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Task.CompletedTask;
        }

        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(accountId));
    }

    private string? GetAccountId()
    {
        return Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
    }
}
