using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

public class AcceptRequest(IUserConnectionService connectionService)
    : Endpoint<UserConnectionRequest.AcceptFriendRequest, Result<UserConnectionResponse.AcceptFriendRequest>>
{
    public override void Configure()
    {
        Post("/api/connections/friends/accept");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override async Task<Result<UserConnectionResponse.AcceptFriendRequest>> ExecuteAsync(UserConnectionRequest.AcceptFriendRequest req, CancellationToken ct)
    {
        return await connectionService.AcceptFriendRequestAsync(req.TargetAccountId, ct);
    }
}