using Rise.Domain.Messages;
using Rise.Domain.Users;

namespace Rise.Services.Notifications;

public interface IMagicBellNotificationService
{
    Task NotifyChatMessageAsync(Message message, IEnumerable<BaseUser> recipients, CancellationToken cancellationToken = default);
}
