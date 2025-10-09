using Rise.Shared.Friends;

namespace Rise.Server.Endpoints.Friends;

public class List(IFriendService friendService) : EndpointWithoutRequest<Result<FriendResponse.Index>>
{
    public override void Configure()
    {
        Get("/api/friends");
    }

    public override Task<Result<FriendResponse.Index>> ExecuteAsync(CancellationToken ct)
    {
        return friendService.GetFriendsAsync(ct);
    }
}
