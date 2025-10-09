using Rise.Shared.Friends;

namespace Rise.Server.Endpoints.Friends;

public class Add(IFriendService friendService) : Endpoint<FriendRequest.Add, Result>
{
    public override void Configure()
    {
        Post("/api/friends/{FriendId:int}");
        DontThrowIfValidationFails();
    }

    public override Task<Result> ExecuteAsync(FriendRequest.Add req, CancellationToken ct)
    {
        return friendService.AddFriendAsync(req, ct);
    }
}
