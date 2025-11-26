using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Rise.Server.Hubs;

[Authorize]
public class Chathub : Hub
{
    public static string GetGroupName(int chatId) => $"chat-{chatId}";
    
    private static readonly ConcurrentDictionary<string, int> OnlineUsers = new();
    
    public override async Task OnConnectedAsync()
    {
        var userId = Context.UserIdentifier!;

        OnlineUsers.AddOrUpdate(userId, 1, (_, count) => count + 1);

        await Clients.All.SendAsync("UserStatusChanged", userId, true); // true = online
        
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var userId = Context.UserIdentifier!;

        if (OnlineUsers.AddOrUpdate(userId, 0, (_, count) => Math.Max(count - 1, 0)) == 0)
        {
            OnlineUsers.TryRemove(userId, out _);
            await Clients.All.SendAsync("UserStatusChanged", userId, false); // false = offline
        }

        await base.OnDisconnectedAsync(exception);
    }
    public Task<List<string>> GetOnlineUsers()
    {
        var onlineUserIds = OnlineUsers.Keys.ToList();
        return Task.FromResult(onlineUserIds);
    }
    public static void ResetOnlineUsers()
    {
        OnlineUsers.Clear();
    }
    public Task JoinChat(int chatId)
    {
        return Groups.AddToGroupAsync(Context.ConnectionId, GetGroupName(chatId));
    }

    public Task LeaveChat(int chatId)
    {
        return Groups.RemoveFromGroupAsync(Context.ConnectionId, GetGroupName(chatId));
    }

    public Task SendMessage(string user, string message)
    {
        return Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public Task SendVoiceMessage(string user, string dataUrl, double durationSeconds)
    {
        return Clients.All.SendAsync("ReceiveVoiceMessage", user, dataUrl, durationSeconds);
    }
}
