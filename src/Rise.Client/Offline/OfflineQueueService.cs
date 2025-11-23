using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;
using System.Net;
using System.Net.Http;

namespace Rise.Client.Offline;

public sealed class OfflineQueueService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IHttpClientFactory _httpClientFactory;
    private IJSObjectReference? _module;
    private DotNetObjectReference<OfflineQueueService>? _dotNetRef;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    public OfflineQueueService(IJSRuntime jsRuntime, IHttpClientFactory httpClientFactory)
    {
        _jsRuntime = jsRuntime;
        _httpClientFactory = httpClientFactory;
    }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();

        _dotNetRef = DotNetObjectReference.Create(this);
        await _module!.InvokeVoidAsync("registerOnlineCallback", cancellationToken, _dotNetRef);

        await ProcessQueueAsync(cancellationToken);
    }

    public async Task<bool> IsOnlineAsync()
    {
        await EnsureModuleAsync();
        return await _module!.InvokeAsync<bool>("isOnline");
    }

    public async Task<int> QueueOperationAsync(string baseAddress, string path, HttpMethod method, object? payload,
        Dictionary<string, string>? headers = null, string? contentType = "application/json", CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();

        var serializedBody = payload is null
            ? null
            : JsonSerializer.Serialize(payload, _serializerOptions);

        var operation = new QueuedOperation
        {
            BaseAddress = baseAddress,
            Path = path,
            Method = method.Method,
            Body = serializedBody,
            ContentType = contentType,
            Headers = headers is null ? null : new Dictionary<string, string>(headers),
            CreatedAt = DateTimeOffset.UtcNow
        };

        return await _module!.InvokeAsync<int>("enqueueOperation", cancellationToken, operation);
    }

    public async Task<IReadOnlyList<QueuedOperation>> GetOperationsAsync(CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        var operations = await _module!.InvokeAsync<QueuedOperation[]>("getOperations", cancellationToken);
        return operations ?? Array.Empty<QueuedOperation>();
    }

    public async Task RemoveOperationAsync(int id, CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();
        await _module!.InvokeVoidAsync("removeOperation", cancellationToken, id);
    }

    [JSInvokable]
    public async Task OnBrowserOnline()
    {
        await ProcessQueueAsync();
    }

    public async Task ProcessQueueAsync(CancellationToken cancellationToken = default)
    {
        if (!await IsOnlineAsync())
        {
            return;
        }

        var operations = await GetOperationsAsync(cancellationToken);

        foreach (var operation in operations)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                using var request = BuildRequest(operation);
                var client = _httpClientFactory.CreateClient("SecureApi");
                var response = await client.SendAsync(request, cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    await RemoveOperationAsync(operation.Id, cancellationToken);
                }
            }
            catch
            {
                // If an item fails we keep the rest in the queue and continue trying the remaining operations.
                // They will be retried on the next online event.
                continue;
            }
        }
    }

    public async Task CacheResponseAsync(HttpRequestMessage request, HttpResponseMessage response,
        CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();

        if (!response.IsSuccessStatusCode)
        {
            return;
        }

        var body = response.Content is null
            ? null
            : await response.Content.ReadAsStringAsync(cancellationToken);

        var headers = new Dictionary<string, string>();

        foreach (var header in response.Headers)
        {
            headers[header.Key] = string.Join(',', header.Value);
        }

        if (response.Content is not null)
        {
            foreach (var header in response.Content.Headers)
            {
                headers[header.Key] = string.Join(',', header.Value);
            }
        }

        var cacheEntry = new CachedResponse
        {
            Key = BuildCacheKey(request.RequestUri?.ToString(), request.Method.Method),
            Status = (int)response.StatusCode,
            Body = body,
            ContentType = response.Content?.Headers.ContentType?.ToString(),
            Headers = headers,
            CreatedAt = DateTimeOffset.UtcNow
        };

        await _module!.InvokeVoidAsync("cacheResponse", cancellationToken, cacheEntry);
    }

    public async Task<HttpResponseMessage?> GetCachedResponseAsync(string? baseAddress, string path, HttpMethod method,
        CancellationToken cancellationToken = default)
    {
        await EnsureModuleAsync();

        var key = BuildCacheKey(BuildAbsoluteUri(baseAddress, path), method.Method);
        var cached = await _module!.InvokeAsync<CachedResponse?>("getCachedResponse", cancellationToken, key);

        if (cached is null)
        {
            return null;
        }

        var response = new HttpResponseMessage((HttpStatusCode)cached.Status)
        {
            Content = new StringContent(cached.Body ?? string.Empty, Encoding.UTF8, cached.ContentType ?? "application/json")
        };

        if (cached.Headers is not null)
        {
            foreach (var header in cached.Headers)
            {
                if (!response.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    response.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return response;
    }

    private static HttpRequestMessage BuildRequest(QueuedOperation operation)
    {
        Uri? baseUri = string.IsNullOrWhiteSpace(operation.BaseAddress) ? null : new Uri(operation.BaseAddress);
        var uri = baseUri is null
            ? new Uri(operation.Path, UriKind.RelativeOrAbsolute)
            : new Uri(baseUri, operation.Path);
        var request = new HttpRequestMessage(new HttpMethod(operation.Method), uri);

        if (operation.Body is not null)
        {
            request.Content = new StringContent(operation.Body, Encoding.UTF8, operation.ContentType ?? "application/json");
        }

        if (operation.Headers is not null)
        {
            foreach (var header in operation.Headers)
            {
                if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value))
                {
                    request.Content ??= new StringContent(string.Empty);
                    request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
                }
            }
        }

        return request;
    }

    private static string BuildCacheKey(string? absoluteUri, string method)
    {
        return string.IsNullOrWhiteSpace(absoluteUri)
            ? method
            : $"{method}:{absoluteUri}";
    }

    private static string BuildAbsoluteUri(string? baseAddress, string path)
    {
        if (Uri.TryCreate(path, UriKind.Absolute, out var absolute))
        {
            return absolute.ToString();
        }

        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            return path;
        }

        var baseUri = new Uri(baseAddress);
        return new Uri(baseUri, path).ToString();
    }

    private async Task EnsureModuleAsync()
    {
        if (_module is not null)
        {
            return;
        }

        _module = await _jsRuntime.InvokeAsync<IJSObjectReference>(
            "import", "./js/offlineQueue.js");
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }

        _dotNetRef?.Dispose();
    }
}
