using Microsoft.AspNetCore.SignalR.Client;
using Rise.Client.RealTime;

namespace Rise.Client.Faker;

internal class FakeHubClient : IHubClient
{
    public HubConnectionState State => throw new NotImplementedException();

    public event Func<Exception?, Task>? Reconnecting;
    public event Func<string?, Task>? Reconnected;
    public event Func<Exception?, Task>? Closed;

    public ValueTask DisposeAsync() => ValueTask.CompletedTask;

    public Task<T> InvokeAsync<T>(string methodName, params object[] args)
    {
        if (methodName == "GetOnlineUsers")
        {
            object result = new List<string> { "user1", "user2" };
            return Task.FromResult((T)result);
        }

        return Task.FromResult(default(T));
    }

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

    public Task SendAsync(string methodName, params object[] args)
    {
        return Task.CompletedTask;
    }

    public Task StartAsync()
    {
        return Task.CompletedTask;
    }

    public Task StopAsync()
    {
        return Task.CompletedTask;
    }
}