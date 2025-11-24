using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

public class RejectRequest(IUserConnectionService connectionService)
    : Endpoint<UserConnectionRequest.RejectFriendRequest, Result<UserConnectionResponse.RejectFriendRequest>>
{
    public override void Configure()
    {
        Delete("/api/connections/friends/reject");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override async Task<Result<UserConnectionResponse.RejectFriendRequest>> ExecuteAsync(UserConnectionRequest.RejectFriendRequest req, CancellationToken ct)
    {
        return await connectionService.RejectFriendRequestAsync(req.TargetAccountId, ct);
    }
}