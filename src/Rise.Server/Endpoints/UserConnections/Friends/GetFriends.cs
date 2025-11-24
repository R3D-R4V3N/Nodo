using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;
namespace Rise.Server.Endpoints.UserConnections.Friends;

/// <summary>
/// List all products.
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="connectionService"></param>
public class GetFriends(IUserConnectionService connectionService) : Endpoint<QueryRequest.SkipTake, Result<UserConnectionResponse.GetFriends>>
{
    public override void Configure()
    {
        Get("/api/connections/friends");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<UserConnectionResponse.GetFriends>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        return connectionService.GetFriendsAsync(req, ct);
    }
}