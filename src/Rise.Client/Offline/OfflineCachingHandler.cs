using System.Net;
using System.Net.Http;

namespace Rise.Client.Offline;

public class OfflineCachingHandler(OfflineQueueService offlineQueueService) : DelegatingHandler
{
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var isOnline = await _offlineQueueService.IsOnlineAsync();

        if (!isOnline)
        {
            var cached = await _offlineQueueService.GetCachedResponseAsync(request.RequestUri?.GetLeftPart(UriPartial.Authority),
                request.RequestUri?.PathAndQuery ?? string.Empty, request.Method, cancellationToken);

            if (cached is not null)
            {
                return cached;
            }

            if (request.Method != HttpMethod.Get)
            {
                throw new HttpRequestException("Offline: request queued");
            }
        }

        HttpResponseMessage response;

        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException)
        {
            var cached = await _offlineQueueService.GetCachedResponseAsync(request.RequestUri?.GetLeftPart(UriPartial.Authority),
                request.RequestUri?.PathAndQuery ?? string.Empty, request.Method, cancellationToken);

            if (cached is not null)
            {
                return cached;
            }

            throw;
        }

        await _offlineQueueService.CacheResponseAsync(request, response, cancellationToken);

        return response;
    }
}
