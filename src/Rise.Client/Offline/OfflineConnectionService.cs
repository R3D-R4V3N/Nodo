using System.Threading;
using System.Threading.Tasks;

namespace Rise.Client.Offline;

public sealed class OfflineConnectionService : IConnectionService
{
    private readonly OfflineQueueService _offlineQueueService;

    public OfflineConnectionService(OfflineQueueService offlineQueueService)
    {
        _offlineQueueService = offlineQueueService;
    }

    public Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        async Task HandleRestoredConnectionAsync()
        {
            await _offlineQueueService.HandleOnlineAsync(cancellationToken);
        }

        _offlineQueueService.WentOnline += HandleRestoredConnectionAsync;
        return Task.CompletedTask;
    }
}
