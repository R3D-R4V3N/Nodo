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

    public Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }

    public Task<Result<string>> AcceptFriendAsync(string requesterAccountId, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>> GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.GetSuggestions>>("/api/connections/suggested", cancellationToken: ct);
        return result!;
    }
}
