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
    }
}
