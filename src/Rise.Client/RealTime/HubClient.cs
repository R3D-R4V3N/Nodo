using Microsoft.AspNetCore.SignalR.Client;

namespace Rise.Client.RealTime;
public class HubClient : IHubClient
{
    private readonly HubConnection _connection;

    public HubClient(HubConnection connection)
    {
        _connection = connection;
    }

    public HubConnectionState State => _connection.State;

    public event Func<Exception?, Task>? Reconnecting
    {
        add => _connection.Reconnecting += value;
        remove => _connection.Reconnecting -= value;
    }

    public event Func<string?, Task>? Reconnected
    {
        add => _connection.Reconnected += value;
        remove => _connection.Reconnected -= value;
    }

    public event Func<Exception?, Task>? Closed
    {
        add => _connection.Closed += value;
        remove => _connection.Closed -= value;
    }

    public Task StartAsync() => _connection.StartAsync();
    public Task StopAsync() => _connection.StopAsync();
    public Task SendAsync(string methodName, params object[] args) 
        => _connection.SendCoreAsync(methodName, args);
    public Task<T> InvokeAsync<T>(string methodName, params object[] args)
        => _connection.InvokeCoreAsync<T>(methodName, args);
    public void On<T>(string methodName, Action<T> handler)
    => _connection.On(methodName, handler);
    public void On<T1, T2>(string methodName, Action<T1, T2> handler)
        => _connection.On(methodName, handler);
    public void On<T1, T2, T3>(string methodName, Action<T1, T2, T3> handler)
        => _connection.On(methodName, handler);
    public void On<T1, T2, T3, T4>(string methodName, Action<T1, T2, T3, T4> handler)
        => _connection.On(methodName, handler);
    public void On<T1, T2, T3, T4, T5>(string methodName, Action<T1, T2, T3, T4, T5> handler)
        => _connection.On(methodName, handler);
    public ValueTask DisposeAsync() => _connection.DisposeAsync();
}
