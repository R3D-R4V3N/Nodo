using Rise.Shared.Friends;

namespace Rise.Server.Endpoints.Friends;

public class Remove(IFriendService friendService) : Endpoint<FriendRequest.Remove, Result>
{
    public override void Configure()
    {
        Delete("/api/friends/{FriendId:int}");
        DontThrowIfValidationFails();
    }

    public override Task<Result> ExecuteAsync(FriendRequest.Remove req, CancellationToken ct)
    {
        return friendService.RemoveFriendAsync(req, ct);
    }
}
