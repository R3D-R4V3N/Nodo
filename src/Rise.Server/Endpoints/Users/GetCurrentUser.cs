using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class GetCurrentUser(IUserContextService userService) : EndpointWithoutRequest<Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Get("/api/users/current");
        Claims(ClaimTypes.NameIdentifier);
        Summary(s =>
        {
            s.Summary = "Get current user";
            s.Description = "Returns profile information for the authenticated user.";
        });
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(CancellationToken ct)
    {
        return userService.GetCurrentUserAsync(ct);
    }
}
