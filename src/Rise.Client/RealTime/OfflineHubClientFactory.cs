namespace Rise.Client.RealTime;

public sealed class OfflineHubClientFactory : IHubClientFactoryStrategy
{
    public HubConnectionAvailability Availability => HubConnectionAvailability.Offline;

    public Task<IHubClient> CreateAsync()
    {
        return Task.FromResult<IHubClient>(OfflineHubClient.Instance);
    }
}
