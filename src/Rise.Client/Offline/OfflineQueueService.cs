using System.Text;
using System.Text.Json;
using Microsoft.JSInterop;

namespace Rise.Client.Offline;

public sealed class OfflineQueueService : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private readonly IHttpClientFactory _httpClientFactory;
    private IJSObjectReference? _onlineCallbackDisposable;
    private IJSObjectReference? _processingIntervalDisposable;
    private IJSObjectReference? _module;
    private DotNetObjectReference<OfflineQueueService>? _dotNetRef;
    private static readonly TimeSpan ProcessingInterval = TimeSpan.FromSeconds(30);
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
        _onlineCallbackDisposable =
            await _module!.InvokeAsync<IJSObjectReference?>("registerOnlineCallback", cancellationToken, _dotNetRef);
        _processingIntervalDisposable = await _module.InvokeAsync<IJSObjectReference?>("registerProcessingInterval",
            cancellationToken, _dotNetRef, (int)ProcessingInterval.TotalMilliseconds);

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
        if (_onlineCallbackDisposable is not null)
        {
            await _onlineCallbackDisposable.DisposeAsync();
        }

        if (_processingIntervalDisposable is not null)
        {
            await _processingIntervalDisposable.DisposeAsync();
        }

        if (_module is not null)
        {
            await _module.DisposeAsync();
        }

        _dotNetRef?.Dispose();
    }
}
