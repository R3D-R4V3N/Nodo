using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

public class Reject(IUserConnectionService connectionService)
    : Endpoint<FriendRequestRejectAction, Result<string>>
{
    public override void Configure()
    {
        Post("/api/connections/reject");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override async Task<Result<string>> ExecuteAsync(FriendRequestRejectAction req, CancellationToken ct)
    {
        return await connectionService.RejectFriendAsync(req.RequesterAccountId, ct);
    }
}

public class FriendRequestRejectAction
{
    public string RequesterAccountId { get; set; } = string.Empty;
}