namespace Rise.Client.RealTime;

public interface IHubClientFactory
{
    Task<IHubClient> CreateAsync();
}