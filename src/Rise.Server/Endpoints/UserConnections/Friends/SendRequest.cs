using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

/// <summary>
/// Adds a new friend connection.
/// </summary>
public class SendRequest(IUserConnectionService connectionService)
    : Endpoint<UserConnectionRequest.SendFriendRequest, Result<UserConnectionResponse.SendFriendRequest>>
{
    public override void Configure()
    {
        Post("/api/connections/friends/add");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override async Task<Result<UserConnectionResponse.SendFriendRequest>> ExecuteAsync(UserConnectionRequest.SendFriendRequest req, CancellationToken ct)
    {
        return await connectionService.SendFriendRequestAsync(req.TargetAccountId, ct);
    }
}