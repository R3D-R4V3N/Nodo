using System.Threading;
using System.Threading.Tasks;

namespace Rise.Services.UserConnections;

public interface IUserConnectionNotificationDispatcher
{
    Task NotifyFriendConnectionsChangedAsync(string accountId, CancellationToken cancellationToken = default);
}
