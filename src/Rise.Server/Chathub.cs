using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;


namespace Rise.Server.Hubs;

public class Chathub : Hub
{
    public async Task SendMessage(string messageId, string user, string message)
    {
        // Stuur het bericht naar alle verbonden clients
        await Clients.All.SendAsync("ReceiveMessage", messageId, user, message);
    }

    public async Task SendVoiceMessage(string messageId, string user, string dataUrl, double durationSeconds)
    {
        await Clients.All.SendAsync("ReceiveVoiceMessage", messageId, user, dataUrl, durationSeconds);
    }
}
