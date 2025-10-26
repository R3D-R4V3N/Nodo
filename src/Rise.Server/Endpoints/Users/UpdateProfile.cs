using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class UpdateProfile(IUserContextService userService) : Endpoint<UserRequest.UpdateProfile, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Put("/api/users/current");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(UserRequest.UpdateProfile req, CancellationToken ct)
    {
        return userService.UpdateProfileAsync(req, ct);
    }
}
