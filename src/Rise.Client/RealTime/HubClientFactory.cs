using Rise.Client.Offline;

namespace Rise.Client.RealTime;

public sealed class HubClientFactory : IHubClientFactory
{
    private readonly OfflineQueueService _offlineQueueService;
    private readonly IReadOnlyDictionary<HubConnectionAvailability, IHubClientFactoryStrategy> _factories;
    private static readonly IReadOnlyDictionary<bool, HubConnectionAvailability> _availabilityByNavigatorState =
        new Dictionary<bool, HubConnectionAvailability>
        {
            [true] = HubConnectionAvailability.Online,
            [false] = HubConnectionAvailability.Offline
        };

    public HubClientFactory(
        OfflineQueueService offlineQueueService,
        IEnumerable<IHubClientFactoryStrategy> factories)
    {
        _offlineQueueService = offlineQueueService;
        _factories = factories.ToDictionary(factory => factory.Availability);
    }

    public async Task<IHubClient> CreateAsync()
    {
        var availability = await ResolveAvailabilityAsync();
        var factory = _factories[availability];
        return await factory.CreateAsync();
    }

    private async Task<HubConnectionAvailability> ResolveAvailabilityAsync()
    {
        var isOnline = await _offlineQueueService.IsOnlineAsync();
        return _availabilityByNavigatorState[isOnline];
    }
}
