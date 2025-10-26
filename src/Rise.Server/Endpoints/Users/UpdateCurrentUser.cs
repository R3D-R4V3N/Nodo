using System.Security.Claims;
using Ardalis.Result;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class UpdateCurrentUser(IUserContextService userService) : Endpoint<UserRequest.UpdateCurrentUser, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Put("/api/users/current");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(UserRequest.UpdateCurrentUser req, CancellationToken ct)
    {
        return userService.UpdateCurrentUserAsync(req, ct);
    }
}
