using System.Net.Http.Json;
using Rise.Shared.Friends;

namespace Rise.Client.Friends;

public interface IFriendApi
{
    Task<Result<FriendResponse.Index>> GetAsync(CancellationToken ct = default);
    Task<Result> AddAsync(int friendId, CancellationToken ct = default);
    Task<Result> RemoveAsync(int friendId, CancellationToken ct = default);
}

public class FriendApi(HttpClient httpClient) : IFriendApi
{
    public async Task<Result<FriendResponse.Index>> GetAsync(CancellationToken ct = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<FriendResponse.Index>>("/api/friends", cancellationToken: ct);
        return result!;
    }

    public async Task<Result> AddAsync(int friendId, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/api/friends/{friendId}", new FriendRequest.Add
        {
            FriendId = friendId
        }, ct);

        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: ct);
        return result!;
    }

    public async Task<Result> RemoveAsync(int friendId, CancellationToken ct = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Delete, $"/api/friends/{friendId}")
        {
            Content = JsonContent.Create(new FriendRequest.Remove
            {
                FriendId = friendId
            })
        };

        var response = await httpClient.SendAsync(request, ct);
        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: ct);
        return result!;
    }
}
