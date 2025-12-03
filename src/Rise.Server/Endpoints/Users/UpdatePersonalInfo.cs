using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class UpdatePersonalInfo(IUserService userService)
    : Endpoint<UserRequest.UpdatePersonalInfo, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Put("/api/users/{accountId}/personal-info");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(UserRequest.UpdatePersonalInfo req, CancellationToken ct)
    {
        var userToChangeAccountId = Route<string>("accountId")!;
        return userService.UpdatePersonalInfoAsync(userToChangeAccountId, req, ct);
    }
}
