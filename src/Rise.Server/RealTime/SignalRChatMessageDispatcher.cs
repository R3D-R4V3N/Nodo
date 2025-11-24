using Microsoft.AspNetCore.SignalR;
using Rise.Server.Hubs;
using Rise.Services.Chats;
using Rise.Shared.Chats;

namespace Rise.Server.RealTime;

public class SignalRChatMessageDispatcher(IHubContext<Chathub> hubContext) : IChatMessageDispatcher
{
    private readonly IHubContext<Chathub> _hubContext = hubContext;

    public Task NotifyMessageCreatedAsync(int chatId, MessageDto.Chat message, CancellationToken cancellationToken = default)
    {
        return _hubContext
            .Clients
            .Group(Chathub.GetGroupName(chatId))
            .SendAsync("MessageCreated", message, cancellationToken);
    }
}
