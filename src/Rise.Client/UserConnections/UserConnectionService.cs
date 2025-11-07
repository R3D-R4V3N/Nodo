using Rise.Shared.Common;
using Rise.Shared.UserConnections;
using System.Net.Http.Json;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient) : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.GetFriends>> 
        GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetFriends>>("/api/connections/friends", cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.SendFriendRequest>> 
        SendFriendRequestAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var body = new UserConnectionRequest.SendFriendRequest() { TargetAccountId = targetAccountId };
    
        var response = await httpClient.PostAsJsonAsync("/api/connections/friends/add", body, ctx);

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

        var response = await httpClient.PostAsJsonAsync("/api/connections/friends/accept", body, ctx);

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
        var body = new UserConnectionRequest.RejectFriendRequest() { TargetAccountId = targetAccountId };
    
        var response = await httpClient.PostAsJsonAsync("/api/connections/friends/reject", body, ctx);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het afwijzen van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserConnectionResponse.RejectFriendRequest>>(cancellationToken: ctx);
        return result!;
        
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>>
        GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ctx = default)
    {
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetSuggestions>>("/api/connections/friends/suggested", cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.CancelFriendRequest>> CancelFriendRequest(string targetAccountId, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"/api/connections/cancel?targetAccountId={targetAccountId}", ct);

        //var response = await httpClient.DeleteAsync(requestUrl, ct);

        // Controleer of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result.Error($"Fout bij het annuleren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserConnectionResponse.CancelFriendRequest>>(cancellationToken: ct);
        return result!;
    }
}
