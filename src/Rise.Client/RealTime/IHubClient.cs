using Microsoft.AspNetCore.SignalR.Client;

namespace Rise.Client.RealTime;

public interface IHubClient : IAsyncDisposable
{
    HubConnectionState State { get; }

    event Func<Exception?, Task>? Reconnecting;
    event Func<string?, Task>? Reconnected;
    event Func<Exception?, Task>? Closed;


    Task StartAsync();
    Task StopAsync();
    Task SendAsync(string methodName, params object[] args);
    Task<T> InvokeAsync<T>(string methodName, params object[] args);
    void On<T>(string methodName, Action<T> handler);
    void On<T1, T2>(string methodName, Action<T1, T2> handler);
    void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler);
    void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler);
    void On<T1, T2, T3, T4, T5>(string methodName, Action<T1, T2, T3, T4, T5> handler);
}