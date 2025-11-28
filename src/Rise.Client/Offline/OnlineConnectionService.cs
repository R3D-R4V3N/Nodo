using System.Threading;
using System.Threading.Tasks;

namespace Rise.Client.Offline;

public sealed class OnlineConnectionService : IConnectionService
{
    private readonly OfflineQueueService _offlineQueueService;

    public OnlineConnectionService(OfflineQueueService offlineQueueService)
    {
        _offlineQueueService = offlineQueueService;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        return _offlineQueueService.HandleOnlineAsync(cancellationToken);
    }
}
