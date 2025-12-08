namespace Rise.Client.Offline;

public interface ICacheService
{
    public Task CacheAsync<T>(string key, T objectToCache, CancellationToken cancellationToken);
    public Task<T?> TryGetCachedAsync<T>(string key, CancellationToken cancellationToken);
    public Task ClearCacheAsync(CancellationToken cancellationToken);
}
