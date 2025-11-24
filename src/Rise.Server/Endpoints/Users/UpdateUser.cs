using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class UpdateUser(IUserService userService)
    : Endpoint<UserRequest.UpdateCurrentUser, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Put("/api/users/{accountId}");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(UserRequest.UpdateCurrentUser req, CancellationToken ct)
    {
        var userToChangeAccountId = Route<string>("accountId")!;
        return userService.UpdateUserAsync(userToChangeAccountId, req, ct);
    }
}
