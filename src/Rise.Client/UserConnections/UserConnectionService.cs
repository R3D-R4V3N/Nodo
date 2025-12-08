using System.Net.Http.Json;
using System.Text.Json;
using Ardalis.Result;
using Microsoft.JSInterop;
using Rise.Client.Offline;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient, OfflineQueueService offlineQueueService, ICacheService cacheService)
    : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.GetFriends>>
        GetFriendsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        try
        {
            var result = await httpClient
                .GetFromJsonAsync<Result<UserConnectionResponse.GetFriends>>("/api/connections/friends", cancellationToken: ctx);

            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                var friendsResponse = result.Value;
                await cacheService.CacheAsync(CacheKeys.FriendsCacheKey, friendsResponse, ctx);
            }

            return result ?? Result<UserConnectionResponse.GetFriends>.Error("Kon de vriendenlijst niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await cacheService
                .TryGetCachedAsync<UserConnectionResponse.GetFriends>(CacheKeys.FriendsCacheKey, ctx);
            
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
            var result = await httpClient
                .GetFromJsonAsync<Result<UserConnectionResponse.GetSuggestions>>("/api/connections/friends/suggested", cancellationToken: ctx);

            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                var friendSuggiestionResponse = result.Value;
                await cacheService.CacheAsync(CacheKeys.FriendSuggestionsCacheKey, friendSuggiestionResponse, ctx);
            }

            return result ?? Result<UserConnectionResponse.GetSuggestions>.Error("Kon de suggesties niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await cacheService
                .TryGetCachedAsync<UserConnectionResponse.GetSuggestions>(CacheKeys.FriendSuggestionsCacheKey, ctx);
            
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
            var result = await httpClient
                .GetFromJsonAsync<Result<UserConnectionResponse.GetFriendRequests>>("/api/connections/friendrequests", cancellationToken: ctx);

            if (result is not null && result.IsSuccess && result.Value is not null)
            {
                var friendRequestsResponse = result.Value;
                await cacheService.CacheAsync(CacheKeys.FriendRequestsCacheKey, friendRequestsResponse, ctx);
            }

            return result ?? Result<UserConnectionResponse.GetFriendRequests>.Error("Kon de verzoeken niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await cacheService
                .TryGetCachedAsync<UserConnectionResponse.GetFriendRequests>(CacheKeys.FriendRequestsCacheKey, ctx);
            
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

        if (!await offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync("/api/connections/friends/add", HttpMethod.Post, body, ctx);
            return Result<UserConnectionResponse.SendFriendRequest>.Error("Geen netwerkverbinding: het vriendschapsverzoek wordt verstuurd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/api/connections/friends/add", body, ctx);
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

        if (!await offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync("/api/connections/friends/accept", HttpMethod.Post, body, ctx);
            return Result<UserConnectionResponse.AcceptFriendRequest>.Error("Geen netwerkverbinding: de actie wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/api/connections/friends/accept", body, ctx);
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
        if (!await offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync($"/api/connections/friends/reject?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.RejectFriendRequest>.Error("Geen netwerkverbinding: de actie is opgeslagen en wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"/api/connections/friends/reject?targetAccountId={targetAccountId}", ctx);
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
        if (!await offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync($"/api/connections/friends/cancel?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.CancelFriendRequest>.Error("Geen netwerkverbinding: de annulatie wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"/api/connections/friends/cancel?targetAccountId={targetAccountId}", ctx);
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
        if (!await offlineQueueService.IsOnlineAsync())
        {
            await QueueOperationAsync($"/api/connections/friends/remove?targetAccountId={targetAccountId}", HttpMethod.Delete, null, ctx);
            return Result<UserConnectionResponse.RemoveFriendRequest>.Error("Geen netwerkverbinding: de actie wordt uitgevoerd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.DeleteAsync($"/api/connections/friends/remove?targetAccountId={targetAccountId}", ctx);
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
        return offlineQueueService.QueueOperationAsync(
            httpClient.BaseAddress?.ToString() ?? string.Empty,
            path,
            method,
            payload,
            cancellationToken: cancellationToken);
    }
}
