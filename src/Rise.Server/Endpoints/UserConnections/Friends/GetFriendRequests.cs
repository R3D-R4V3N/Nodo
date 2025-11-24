using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;
namespace Rise.Server.Endpoints.UserConnections.Friends;

/// <summary>
/// List all products.
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="connectionService"></param>
public class GetFriendRequests(IUserConnectionService connectionService) : Endpoint<QueryRequest.SkipTake, Result<UserConnectionResponse.GetFriendRequests>>
{
    public override void Configure()
    {
        Get("/api/connections/friendrequests");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<UserConnectionResponse.GetFriendRequests>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        return connectionService.GetFriendRequestsAsync(req, ct);
    }
}