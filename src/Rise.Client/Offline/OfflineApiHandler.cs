using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.Extensions.Logging;

namespace Rise.Client.Offline;

public sealed class OfflineApiHandler : DelegatingHandler
{
    private static readonly HttpMethod[] MutatingMethods =
    {
        HttpMethod.Post,
        HttpMethod.Put,
        HttpMethod.Patch,
        HttpMethod.Delete
    };

    private readonly IndexedDbCacheService _cacheService;
    private readonly OfflineQueueService _offlineQueueService;
    private readonly ILogger<OfflineApiHandler> _logger;

    public OfflineApiHandler(
        IndexedDbCacheService cacheService,
        OfflineQueueService offlineQueueService,
        ILogger<OfflineApiHandler> logger)
    {
        _cacheService = cacheService;
        _offlineQueueService = offlineQueueService;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await base.SendAsync(request, cancellationToken);

            if (request.Method == HttpMethod.Get && response.IsSuccessStatusCode)
            {
                await _cacheService.CacheResponseAsync(request, response, cancellationToken);
            }

            return response;
        }
        catch (HttpRequestException ex)
        {
            if (request.Method == HttpMethod.Get)
            {
                var cachedResponse = await _cacheService.TryGetCachedResponseAsync(request, cancellationToken);
                if (cachedResponse is not null)
                {
                    _logger.LogWarning(ex, "Connectivity issue detected for {Url}. Returning cached response instead.", request.RequestUri);
                    return cachedResponse;
                }
            }

            if (IsMutating(request.Method))
            {
                return await HandleMutatingFailureAsync(request, ex, cancellationToken);
            }

            throw;
        }
    }

    private static bool IsMutating(HttpMethod method) => MutatingMethods.Any(m => m == method);

    private async Task<HttpResponseMessage> HandleMutatingFailureAsync(HttpRequestMessage request, HttpRequestException exception, CancellationToken cancellationToken)
    {
        _logger.LogWarning(exception, "Connectivity issue detected for {Method} {Url}. Applying retry/backoff before queuing.", request.Method, request.RequestUri);

        await Task.Delay(TimeSpan.FromMilliseconds(300), cancellationToken);

        var retryRequest = await CloneRequestAsync(request, cancellationToken);

        try
        {
            return await base.SendAsync(retryRequest, cancellationToken);
        }
        catch (HttpRequestException retryException)
        {
            _logger.LogWarning(retryException, "Retry failed for {Method} {Url}. Enqueuing for offline processing.", request.Method, request.RequestUri);
            await EnqueueAsync(request, cancellationToken);

            return new HttpResponseMessage(HttpStatusCode.Accepted)
            {
                RequestMessage = request,
                Content = new StringContent("Request queued for offline processing.", Encoding.UTF8, "text/plain")
            };
        }
    }

    private async Task EnqueueAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.RequestUri is null)
        {
            return;
        }

        var payload = request.Content is null
            ? null
            : await request.Content.ReadAsStringAsync(cancellationToken);

        var headers = new Dictionary<string, string>();
        foreach (var header in request.Headers)
        {
            headers[header.Key] = string.Join(",", header.Value);
        }

        if (request.Content is not null)
        {
            foreach (var header in request.Content.Headers)
            {
                headers[header.Key] = string.Join(",", header.Value);
            }
        }

        var baseAddress = request.RequestUri.GetLeftPart(UriPartial.Authority);
        var path = request.RequestUri.PathAndQuery;

        await _offlineQueueService.QueueOperationAsync(baseAddress, path, request.Method, payload, headers, request.Content?.Headers?.ContentType?.ToString(), cancellationToken);
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri)
        {
            Version = request.Version,
            VersionPolicy = request.VersionPolicy
        };

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        if (request.Content is not null)
        {
            var content = await request.Content.ReadAsByteArrayAsync(cancellationToken);
            var ms = new MemoryStream(content);
            clone.Content = new StreamContent(ms);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        return clone;
    }
}
