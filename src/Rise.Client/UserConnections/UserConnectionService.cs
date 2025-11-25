using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.JSInterop;
using Rise.Client.Offline;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient, OfflineQueueService offlineQueueService, IJSRuntime jsRuntime)
    : IUserConnectionService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    private readonly IJSRuntime _jsRuntime = jsRuntime;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
    };

    private const string FriendsCacheKey = "offline-cache:connections:friends";
    private const string RequestsCacheKey = "offline-cache:connections:requests";
    private const string SuggestionsCacheKey = "offline-cache:connections:suggestions";

    public async Task<Result<UserConnectionResponse.GetFriends>>
        GetFriendsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        try
        {
            var result = await _httpClient
                .GetFromJsonAsync<Result<UserConnectionResponse.GetFriends>>("/api/connections/friends", cancellationToken: ctx);

            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                await CacheFriendsAsync(result.Value, ctx);
            }

            return result ?? Result<UserConnectionResponse.GetFriends>.Error("Kon de vriendenlijst niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await TryGetCachedFriendsAsync(ctx);
            if (cached is not null)
            {
                return Result<UserConnectionResponse.GetFriends>.Success(cached,
                    "Offline: eerder geladen vrienden worden getoond.");
            }

            return Result<UserConnectionResponse.GetFriends>.Error(
                "Offline: de vriendenlijst is niet beschikbaar zonder eerdere verbinding.");
        }
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>>
        GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ctx = default)
    {
        try
        {
            var result = await _httpClient
                .GetFromJsonAsync<Result<UserConnectionResponse.GetSuggestions>>("/api/connections/friends/suggested", cancellationToken: ctx);

            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                await CacheSuggestionsAsync(result.Value, ctx);
            }

            return result ?? Result<UserConnectionResponse.GetSuggestions>.Error("Kon de suggesties niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await TryGetCachedSuggestionsAsync(ctx);
            if (cached is not null)
            {
                return Result<UserConnectionResponse.GetSuggestions>.Success(cached,
                    "Offline: eerder geladen suggesties worden getoond.");
            }

            return Result<UserConnectionResponse.GetSuggestions>.Error(
                "Offline: suggesties zijn niet beschikbaar zonder eerdere verbinding.");
        }
    }

    public async Task<Result<UserConnectionResponse.GetFriendRequests>>
        GetFriendRequestsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        try
        {
            var result = await _httpClient
                .GetFromJsonAsync<Result<UserConnectionResponse.GetFriendRequests>>("/api/connections/friendrequests", cancellationToken: ctx);

            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                await CacheFriendRequestsAsync(result.Value, ctx);
            }

            return result ?? Result<UserConnectionResponse.GetFriendRequests>.Error("Kon de verzoeken niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await TryGetCachedFriendRequestsAsync(ctx);
            if (cached is not null)
            {
                return Result<UserConnectionResponse.GetFriendRequests>.Success(cached,
                    "Offline: eerder geladen verzoeken worden getoond.");
            }

            return Result<UserConnectionResponse.GetFriendRequests>.Error(
                "Offline: vriendschapsverzoeken zijn niet beschikbaar zonder eerdere verbinding.");
        }
    }

    public async Task<Result<UserConnectionResponse.SendFriendRequest>>
        SendFriendRequestAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var body = new UserConnectionRequest.SendFriendRequest() { TargetAccountId = targetAccountId };

        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync("/api/connections/friends/add", HttpMethod.Post, body, ctx);
            return Result<UserConnectionResponse.SendFriendRequest>.Error("Geen netwerkverbinding: het vriendschapsverzoek wordt verstuurd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/api/connections/friends/add", body, ctx);
        }
        catch (HttpRequestException)
        {
            await QueueOperationAsync("/api/connections/friends/add", HttpMethod.Post, body, ctx);
            return Result<UserConnectionResponse.SendFriendRequest>.Error("Kon geen verbinding maken: het vriendschapsverzoek is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        // checken of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het accepteren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserConnectionResponse.SendFriendRequest>>(cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.AcceptFriendRequest>>
        AcceptFriendRequestAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var body = new UserConnectionRequest.AcceptFriendRequest() { TargetAccountId = targetAccountId };

        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync("/api/connections/friends/accept", HttpMethod.Post, body, ctx);
            return Result<UserConnectionResponse.AcceptFriendRequest>.Error("Geen netwerkverbinding: de actie wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("/api/connections/friends/accept", body, ctx);
        }
        catch (HttpRequestException)
        {
            await QueueOperationAsync("/api/connections/friends/accept", HttpMethod.Post, body, ctx);
            return Result<UserConnectionResponse.AcceptFriendRequest>.Error("Kon geen verbinding maken: de actie is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        // checken of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het accepteren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserConnectionResponse.AcceptFriendRequest>>(cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.RejectFriendRequest>>
        RejectFriendRequestAsync(string targetAccountId, CancellationToken ctx = default)
    {
        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync($"/api/connections/friends/reject?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.RejectFriendRequest>.Error("Geen netwerkverbinding: de actie is opgeslagen en wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.DeleteAsync($"/api/connections/friends/reject?targetAccountId={targetAccountId}", ctx);
        }
        catch (HttpRequestException)
        {
            await QueueOperationAsync($"/api/connections/friends/reject?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.RejectFriendRequest>.Error("Kon geen verbinding maken: de actie is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het afwijzen van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserConnectionResponse.RejectFriendRequest>>(cancellationToken: ctx);
        return result!;

    }

    public async Task<Result<UserConnectionResponse.CancelFriendRequest>>
        CancelFriendRequest(string targetAccountId, CancellationToken ctx = default)
    {
        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync($"/api/connections/friends/cancel?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.CancelFriendRequest>.Error("Geen netwerkverbinding: de annulatie wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.DeleteAsync($"/api/connections/friends/cancel?targetAccountId={targetAccountId}", ctx);
        }
        catch (HttpRequestException)
        {
            await QueueOperationAsync($"/api/connections/friends/cancel?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.CancelFriendRequest>.Error("Kon geen verbinding maken: de annulatie is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        // Controleer of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het annuleren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserConnectionResponse.CancelFriendRequest>>(cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.RemoveFriendRequest>>
        RemoveFriendAsync(string targetAccountId, CancellationToken ctx = default)
    {
        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync($"/api/connections/friends/remove?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.RemoveFriendRequest>.Error("Geen netwerkverbinding: de actie wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.DeleteAsync($"/api/connections/friends/remove?targetAccountId={targetAccountId}", ctx);
        }
        catch (HttpRequestException)
        {
            await QueueOperationAsync($"/api/connections/friends/remove?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.RemoveFriendRequest>.Error("Kon geen verbinding maken: de actie is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        // Controleer of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het verwijderen van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserConnectionResponse.RemoveFriendRequest>>(cancellationToken: ctx);
        return result!;
    }

    private Task QueueOperationAsync(string path, HttpMethod method, object? payload, CancellationToken cancellationToken)
    {
        return _offlineQueueService.QueueOperationAsync(
            _httpClient.BaseAddress?.ToString() ?? string.Empty,
            path,
            method,
            payload,
            cancellationToken: cancellationToken);
    }

    private async Task CacheFriendsAsync(UserConnectionResponse.GetFriends friends, CancellationToken cancellationToken)
    {
        await CacheAsync(FriendsCacheKey, friends, cancellationToken);
    }

    private async Task CacheFriendRequestsAsync(UserConnectionResponse.GetFriendRequests requests, CancellationToken cancellationToken)
    {
        await CacheAsync(RequestsCacheKey, requests, cancellationToken);
    }

    private async Task CacheSuggestionsAsync(UserConnectionResponse.GetSuggestions suggestions, CancellationToken cancellationToken)
    {
        await CacheAsync(SuggestionsCacheKey, suggestions, cancellationToken);
    }

    private async Task CacheAsync<T>(string key, T payload, CancellationToken cancellationToken)
    {
        try
        {
            var serialized = JsonSerializer.Serialize(payload, _serializerOptions);
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", cancellationToken, key, serialized);
        }
        catch
        {
            // Best-effort caching: failures should not break the UX.
        }
    }

    private Task<UserConnectionResponse.GetFriends?> TryGetCachedFriendsAsync(CancellationToken cancellationToken)
    {
        return TryGetCachedAsync<UserConnectionResponse.GetFriends>(FriendsCacheKey, cancellationToken);
    }

    private Task<UserConnectionResponse.GetFriendRequests?> TryGetCachedFriendRequestsAsync(CancellationToken cancellationToken)
    {
        return TryGetCachedAsync<UserConnectionResponse.GetFriendRequests>(RequestsCacheKey, cancellationToken);
    }

    private Task<UserConnectionResponse.GetSuggestions?> TryGetCachedSuggestionsAsync(CancellationToken cancellationToken)
    {
        return TryGetCachedAsync<UserConnectionResponse.GetSuggestions>(SuggestionsCacheKey, cancellationToken);
    }

    private async Task<T?> TryGetCachedAsync<T>(string key, CancellationToken cancellationToken)
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
}
