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
}