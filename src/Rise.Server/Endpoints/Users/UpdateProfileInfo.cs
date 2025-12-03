using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class UpdateProfileInfo(IUserService userService)
    : Endpoint<UserRequest.UpdateProfileInfo, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Put("/api/users/{accountId}/profile");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(UserRequest.UpdateProfileInfo req, CancellationToken ct)
    {
        var userToChangeAccountId = Route<string>("accountId")!;
        return userService.UpdateProfileInfoAsync(userToChangeAccountId, req, ct);
    }
}
