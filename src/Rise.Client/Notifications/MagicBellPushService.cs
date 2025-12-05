using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Rise.Client.Notifications;

public class MagicBellPushService(IJSRuntime jsRuntime) : IMagicBellPushService, IAsyncDisposable
{
    private readonly Lazy<Task<IJSObjectReference>> _moduleTask = new(() =>
        jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/magicBellPush.js").AsTask());

    public async Task<bool> SubscribeAsync(string externalUserId, string? email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(externalUserId))
        {
            return false;
        }

        var module = await _moduleTask.Value;
        return await module.InvokeAsync<bool>(
            "subscribeToMagicBell",
            cancellationToken,
            externalUserId,
            email);
    }

    public async ValueTask DisposeAsync()
    {
        if (_moduleTask.IsValueCreated)
        {
            var module = await _moduleTask.Value;
            await module.DisposeAsync();
        }
    }
}
