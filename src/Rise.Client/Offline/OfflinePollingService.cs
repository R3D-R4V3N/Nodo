using System;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Client.Offline;

public sealed class OfflinePollingService : IAsyncDisposable
{
    private readonly OfflineQueueService _offlineQueueService;
    private PeriodicTimer? _timer;
    private CancellationTokenSource? _cts;
    private Task? _pollingTask;
    private readonly TimeSpan _defaultInterval = TimeSpan.FromSeconds(15);

    public OfflinePollingService(OfflineQueueService offlineQueueService)
    {
        _offlineQueueService = offlineQueueService;
    }

    public void Start(TimeSpan? interval = null)
    {
        if (_pollingTask is not null)
        {
            return;
        }

        _cts = new CancellationTokenSource();
        _timer = new PeriodicTimer(interval ?? _defaultInterval);
        _pollingTask = Task.Run(() => PollAsync(_cts.Token));
    }

    private async Task PollAsync(CancellationToken cancellationToken)
    {
        if (_timer is null)
        {
            return;
        }

        try
        {
            while (await _timer.WaitForNextTickAsync(cancellationToken))
            {
                await _offlineQueueService.HandleOnlineAsync(cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            // Swallow cancellation exceptions when stopping the polling loop.
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts?.Cancel();

        if (_pollingTask is not null)
        {
            try
            {
                await _pollingTask;
            }
            catch (OperationCanceledException)
            {
                // Expected when stopping.
            }
        }

        _timer?.Dispose();
        _cts?.Dispose();
    }
}
