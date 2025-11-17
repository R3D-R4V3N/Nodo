using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;



public class DeleteFriend(IUserConnectionService connectionService)
    : Endpoint<UserConnectionRequest.RemoveFriendRequest, Result<UserConnectionResponse.RemoveFriendRequest>>
{
    public override void Configure()
    {
        Delete("/api/connections/friends/remove");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override async Task<Result<UserConnectionResponse.RemoveFriendRequest>> ExecuteAsync(UserConnectionRequest.RemoveFriendRequest req, CancellationToken ct)
    {
        return await connectionService.RemoveFriendAsync(req.TargetAccountId, ct);
    }
}