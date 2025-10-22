using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class GetCurrentUser(IUserService userService) : EndpointWithoutRequest<Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Get("/api/users/current");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(CancellationToken ct)
    {
        return userService.GetCurrentUserAsync(ct);
    }
}
