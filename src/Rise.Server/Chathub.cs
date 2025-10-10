<<<<<<< HEAD
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace Rise.Server.Hubs;

[Authorize]
public class Chathub : Hub
{
    public static string GetGroupName(int chatId) => $"chat-{chatId}";

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
=======
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace Rise.Server.Hubs;

public class Chathub : Hub
{
    public async Task SendMessage(string user, string message)
    {
        // Stuur het bericht naar alle verbonden clients
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    public async Task SendVoiceMessage(string user, string dataUrl, double durationSeconds)
    {
        await Clients.All.SendAsync("ReceiveVoiceMessage", user, dataUrl, durationSeconds);
>>>>>>> origin/main
    }
}
