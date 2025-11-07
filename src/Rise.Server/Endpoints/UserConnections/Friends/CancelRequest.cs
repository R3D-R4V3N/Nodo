using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

public class CancelRequest(IUserConnectionService connectionService)
    : Endpoint<UserConnectionRequest.CancelFriendRequest, Result<UserConnectionResponse.CancelFriendRequest>>
{
    public override void Configure()
    {
        Delete("/api/connections/cancel");
    }

    public override async Task<Result<UserConnectionResponse.CancelFriendRequest>> ExecuteAsync(UserConnectionRequest.CancelFriendRequest req, CancellationToken ct)
    {
        return await connectionService.CancelFriendRequest(req.TargetAccountId, ct);
    }
}