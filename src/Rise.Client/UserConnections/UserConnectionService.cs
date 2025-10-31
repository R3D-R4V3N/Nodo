using Rise.Shared.Common;
using Rise.Shared.UserConnections;
using System.Net.Http.Json;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient) : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(
        QueryRequest.SkipTake request, 
        CancellationToken ctx = default
    )
    {
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetFriends>>("/api/connections/friends", cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var body = new { targetAccountId = targetAccountId };
    
        var response = await httpClient.PostAsJsonAsync("/api/connections/add", body, ctx);

        // checken of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error($"Fout bij het accepteren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<string>>(cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<string>> AcceptFriendAsync(string requesterAccountId, CancellationToken ctx = default)
    {
        var body = new { targetAccountId = requesterAccountId };
    
        var response = await httpClient.PostAsJsonAsync("/api/connections/add", body, ctx);

        // checken of de API een geldige response gaf
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error($"Fout bij het accepteren van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<string>>(cancellationToken: ctx);
        return result!;
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>> GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetSuggestions>>("/api/connections/suggested", cancellationToken: ct);
        return result!;
    }

    public async Task<Result<string>> RejectFriendAsync(string reqRequesterAccountId, CancellationToken ct = default)
    {
        var body = new { RequesterAccountId = reqRequesterAccountId };
    
        var response = await httpClient.PostAsJsonAsync("/api/connections/reject", body, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error($"Fout bij het afwijzen van vriendschap: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<string>>(cancellationToken: ct);
        return result!;
        
    }
}
