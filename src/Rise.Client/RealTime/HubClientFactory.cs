using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Rise.Client.RealTime;

public class HubClientFactory : IHubClientFactory
{
    private readonly NavigationManager _nav;

    public HubClientFactory(NavigationManager nav) => _nav = nav;

    public IHubClient Create()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(_nav.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect()
            .Build();

        return new HubClient(connection);
    }
}