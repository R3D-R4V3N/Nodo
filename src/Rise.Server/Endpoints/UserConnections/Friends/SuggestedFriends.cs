using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

public class SuggestedFriends(IUserConnectionService connectionService)
    : Endpoint<QueryRequest.SkipTake, Result<UserConnectionResponse.GetSuggestions>>
{
    public override void Configure()
    {
        Get("/api/connections/suggested");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<UserConnectionResponse.GetSuggestions>> ExecuteAsync(QueryRequest.SkipTake req, CancellationToken ct)
    {
        return connectionService.GetSuggestedFriendsAsync(req, ct);
    }
}