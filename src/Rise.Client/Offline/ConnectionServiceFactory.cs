using System.Threading;
using System.Threading.Tasks;

namespace Rise.Client.Offline;

public sealed class ConnectionServiceFactory
{
    private readonly OfflineQueueService _offlineQueueService;

    public ConnectionServiceFactory(OfflineQueueService offlineQueueService)
    {
        _offlineQueueService = offlineQueueService;
    }

    public async Task<IConnectionService> CreateAsync(CancellationToken cancellationToken = default)
    {
        var isOnline = await _offlineQueueService.IsOnlineAsync();
        return isOnline
            ? new OnlineConnectionService(_offlineQueueService)
            : new OfflineConnectionService(_offlineQueueService);
    }
}
