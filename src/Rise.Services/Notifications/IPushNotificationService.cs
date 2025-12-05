using System.Threading;
using System.Threading.Tasks;
using Rise.Shared.Chats;

namespace Rise.Services.Notifications;

public interface IPushNotificationService
{
    Task SendChatMessageNotificationAsync(int chatId, MessageDto.Chat message, CancellationToken cancellationToken = default);
}
