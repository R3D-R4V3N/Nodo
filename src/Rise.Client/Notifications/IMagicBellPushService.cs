using System.Threading;
using System.Threading.Tasks;

namespace Rise.Client.Notifications;

public interface IMagicBellPushService
{
    Task<bool> SubscribeAsync(string externalUserId, string? email, CancellationToken cancellationToken = default);
}
