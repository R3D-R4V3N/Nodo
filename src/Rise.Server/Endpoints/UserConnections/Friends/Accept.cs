using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

public class Accept(IUserConnectionService connectionService)
    : Endpoint<FriendRequestAction, Result<string>>
{
    public override void Configure()
    {
        Post("/api/connections/accept");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override async Task<Result<string>> ExecuteAsync(FriendRequestAction req, CancellationToken ct)
    {
        return await connectionService.AddFriendAsync(req.RequesterAccountId, ct);
    }
}

public class FriendRequestAction
{
    public string RequesterAccountId { get; set; } = string.Empty;
}