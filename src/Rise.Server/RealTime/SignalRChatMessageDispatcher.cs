using Microsoft.AspNetCore.SignalR;
using Rise.Server.Hubs;
using Rise.Services.Chats;
using Rise.Services.Notifications;
using Rise.Shared.Chats;

namespace Rise.Server.RealTime;

public class SignalRChatMessageDispatcher(
    IHubContext<Chathub> hubContext,
    IPushNotificationService pushNotificationService) : IChatMessageDispatcher
{
    private readonly IHubContext<Chathub> _hubContext = hubContext;
    private readonly IPushNotificationService _pushNotificationService = pushNotificationService;

    public async Task NotifyMessageCreatedAsync(int chatId, MessageDto.Chat message, CancellationToken cancellationToken = default)
    {
        await _hubContext
            .Clients
            .Group(Chathub.GetGroupName(chatId))
            .SendAsync("MessageCreated", message, cancellationToken);

        await _pushNotificationService.SendChatMessageNotificationAsync(chatId, message, cancellationToken);
    }
}
