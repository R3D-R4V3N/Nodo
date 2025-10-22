using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Server.Endpoints.UserConnections.Friends;

/// <summary>
/// Adds a new friend connection.
/// </summary>
public class Add(IUserConnectionService connectionService)
    : Endpoint<AddFriendRequest, Result<string>>
{
    public override void Configure()
    {
        Post("/api/connections/add");
    }

    public override async Task<Result<string>> ExecuteAsync(AddFriendRequest req, CancellationToken ct)
    {
        return await connectionService.AddFriendAsync(req.TargetAccountId, ct);
    }
}

/// <summary>
/// Request model for adding a friend.
/// </summary>
public class AddFriendRequest
{
    public string TargetAccountId { get; set; } = string.Empty;
}