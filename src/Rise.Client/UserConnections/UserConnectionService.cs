using Rise.Client.Offline;
using Rise.Shared.Common;
using Rise.Shared.UserConnections;
using System.Net.Http.Json;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient, OfflineQueueService offlineQueueService) : IUserConnectionService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;

    public async Task<Result<UserConnectionResponse.GetFriends>>
        GetFriendsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var result = await _httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetFriends>>("/api/connections/friends", cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>>
        GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ctx = default)
    {
        var result = await _httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetSuggestions>>("/api/connections/friends/suggested", cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.GetFriendRequests>>
        GetFriendRequestsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var result = await _httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetFriendRequests>>("/api/connections/friendrequests", cancellationToken: ctx);
        return result!;
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
}
