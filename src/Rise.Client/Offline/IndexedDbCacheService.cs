using System.Text;
using Microsoft.JSInterop;

namespace Rise.Client.Offline;

public sealed class IndexedDbCacheService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public IndexedDbCacheService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task CacheResponseAsync(HttpRequestMessage request, HttpResponseMessage response, CancellationToken cancellationToken = default)
    {
        if (request.RequestUri is null)
        {
            return;
        }

        await EnsureModuleAsync();

        var content = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(cancellationToken);

        var headers = new Dictionary<string, string[]>();

        foreach (var header in response.Headers)
        {
            headers[header.Key] = header.Value.ToArray();
        }

        if (response.Content is not null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = header.Value.ToArray();
            }
        }

        await _module!.InvokeVoidAsync(
            "setCachedResponse",
            cancellationToken,
            new CachedResponse
            {
                Url = request.RequestUri.ToString(),
                Status = (int)response.StatusCode,
                Body = content,
                Headers = headers,
                ContentType = response.Content?.Headers?.ContentType?.ToString()
            });
    }

    public async Task<HttpResponseMessage?> TryGetCachedResponseAsync(HttpRequestMessage request, CancellationToken cancellationToken = default)
    {
        if (request.RequestUri is null)
        {
            return null;
        }

        await EnsureModuleAsync();

        var cached = await _module!.InvokeAsync<CachedResponse?>("getCachedResponse", cancellationToken, request.RequestUri.ToString());

        if (cached is null)
        {
            return null;
        }

        var message = new HttpResponseMessage((System.Net.HttpStatusCode)cached.Status)
        {
            RequestMessage = request
        };

        if (cached.Body is not null)
        {
            message.Content = new StringContent(cached.Body, Encoding.UTF8, cached.ContentType ?? "application/json");
        }

        if (cached.Headers is not null)
        {
            foreach (var header in cached.Headers)
            {
                if (!message.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    message.Content ??= new StringContent(string.Empty);
                    message.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return message;
    }

    private async Task EnsureModuleAsync()
    {
        if (_module is not null)
        {
            return;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/offlineCache.js");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }

    private sealed class CachedResponse
    {
        public string? Url { get; set; }
        public int Status { get; set; }
        public string? Body { get; set; }
        public string? ContentType { get; set; }
        public Dictionary<string, string[]>? Headers { get; set; }
    }
}
