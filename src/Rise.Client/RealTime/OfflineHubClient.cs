using Microsoft.AspNetCore.SignalR.Client;

namespace Rise.Client.RealTime;

public sealed class OfflineHubClient : IHubClient
{
    public static OfflineHubClient Instance { get; } = new();

    private OfflineHubClient()
    {
    }

    public HubConnectionState State => HubConnectionState.Disconnected;

    public event Func<Exception?, Task>? Reconnecting
    {
        add { }
        remove { }
    }

    public event Func<string?, Task>? Reconnected
    {
        add { }
        remove { }
    }

    public event Func<Exception?, Task>? Closed
    {
        add { }
        remove { }
    }

    public Task StartAsync() => Task.CompletedTask;

    public Task StopAsync() => Task.CompletedTask;

    public Task SendAsync(string methodName, params object[] args) => Task.CompletedTask;

    public Task<T> InvokeAsync<T>(string methodName, params object[] args) => Task.FromResult(default(T)!);

    public void On<T>(string methodName, Action<T> handler)
    {
    }

    public void On<T1, T2>(string methodName, Action<T1, T2> handler)
    {
    }

    public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler)
    {
    }

    public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler)
    {
    }

    public void On<T1, T2, T3, T4, T5>(string methodName, Action<T1, T2, T3, T4, T5> handler)
    {
    }

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;
}
