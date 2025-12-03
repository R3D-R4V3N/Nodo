using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class UpdatePreferences(IUserService userService)
    : Endpoint<UserRequest.UpdatePreferences, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Put("/api/users/{accountId}/preferences");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(UserRequest.UpdatePreferences req, CancellationToken ct)
    {
        var userToChangeAccountId = Route<string>("accountId")!;
        return userService.UpdatePreferencesAsync(userToChangeAccountId, req, ct);
    }
}
