using Rise.Shared.Common;
using Rise.Shared.UserConnections;
using System.Net.Http.Json;

namespace Rise.Client.UserConnections;

public class UserConnectionService(HttpClient httpClient) : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.Index>> GetFriendIndexAsync(
        QueryRequest.SkipTake request, 
        CancellationToken ctx = default
    )
    {
        var result = await httpClient
            .GetFromJsonAsync<Result<UserConnectionResponse.Index>>("/api/connections", cancellationToken: ctx);
        return result!;
    }
}
