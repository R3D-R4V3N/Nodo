using Microsoft.JSInterop;
using System.Text.Json;
using System.Threading;

namespace Rise.Client.Offline;

public class CacheService(IJSRuntime jsRuntime) : ICacheService
{
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private static readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public async Task CacheAsync<T>(string key, T objectToCache, CancellationToken cancellationToken)
    {
        try
        {
            var payload = JsonSerializer.Serialize(objectToCache, _serializerOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, payload);
        }
        catch
        {
            // Failing to cache should not break the UX.
        }
    }

    public async Task<T?> TryGetCachedAsync<T>(string key, CancellationToken cancellationToken)
    {
        try
        {
            var cached = await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", cancellationToken, key);
            
            if (string.IsNullOrWhiteSpace(cached))
            {
                return default;
            }

            return JsonSerializer.Deserialize<T>(cached, _serializerOptions);
        }
        catch
        {
            return default;
        }
    }

    public async Task ClearCacheAsync(CancellationToken cancellationToken)
    {
        await _jsRuntime.InvokeAsync<string?>("localStorage.clear", cancellationToken);
    }
}
