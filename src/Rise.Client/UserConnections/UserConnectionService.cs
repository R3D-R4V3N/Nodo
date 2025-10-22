using Rise.Shared.Common;
using Rise.Shared.UserConnections;
using System.Net.Http.Json;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient) : IUserConnectionService
{
<<<<<<< HEAD
    public async Task<Result<UserConnectionResponse.Index>> GetFriendIndexAsync(
=======
    public async Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
        QueryRequest.SkipTake request, 
        CancellationToken ctx = default
    )
    {
        var result = await httpClient
<<<<<<< HEAD
            .GetFromJsonAsync<Result<UserConnectionResponse.Index>>("/api/connections", cancellationToken: ctx);
        return result!;
    }
=======
            .GetFromJsonAsync<Result<UserConnectionResponse.GetFriends>>("/api/connections/friends", cancellationToken: ctx);
        return result!;
    }

    public Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default)
    {
        throw new NotImplementedException();
    }
>>>>>>> codex/add-alert-message-for-supervisor-monitoring
}
