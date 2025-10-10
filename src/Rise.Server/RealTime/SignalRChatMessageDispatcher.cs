using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using Rise.Server.Hubs;
using Rise.Services.Chats;
using Rise.Shared.Chats;

namespace Rise.Server.RealTime;

public class SignalRChatMessageDispatcher(IHubContext<Chathub> hubContext) : IChatMessageDispatcher
{
    private readonly IHubContext<Chathub> _hubContext = hubContext;

    public Task NotifyMessageCreatedAsync(int chatId, MessageDto message, CancellationToken cancellationToken = default)
    {
        return _hubContext
            .Clients
            .Group(Chathub.GetGroupName(chatId))
            .SendAsync("MessageCreated", message, cancellationToken);
    }

    public Task NotifyEmergencyStatusChangedAsync(int chatId, ChatEmergencyStatusDto status, CancellationToken cancellationToken = default)
    {
        return _hubContext
            .Clients
            .Group(Chathub.GetGroupName(chatId))
            .SendAsync("EmergencyStatusChanged", status, cancellationToken);
    }
}
