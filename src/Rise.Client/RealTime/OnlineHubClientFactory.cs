using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;

namespace Rise.Client.RealTime;

public sealed class OnlineHubClientFactory : IHubClientFactoryStrategy
{
    private readonly NavigationManager _navigationManager;

    public OnlineHubClientFactory(NavigationManager navigationManager)
    {
        _navigationManager = navigationManager;
    }

    public HubConnectionAvailability Availability => HubConnectionAvailability.Online;

    public Task<IHubClient> CreateAsync()
    {
        var connection = new HubConnectionBuilder()
            .WithUrl(_navigationManager.ToAbsoluteUri("/chathub"))
            .WithAutomaticReconnect()
            .Build();

        return Task.FromResult<IHubClient>(new HubClient(connection));
    }
}
