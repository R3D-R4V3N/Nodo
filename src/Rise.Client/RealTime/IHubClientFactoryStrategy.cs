namespace Rise.Client.RealTime;

public interface IHubClientFactoryStrategy
{
    HubConnectionAvailability Availability { get; }

    Task<IHubClient> CreateAsync();
}
