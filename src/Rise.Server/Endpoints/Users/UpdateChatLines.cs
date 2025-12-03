using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class UpdateChatLines(IUserService userService)
    : Endpoint<UserRequest.UpdateChatLines, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Put("/api/users/{accountId}/chatlines");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(UserRequest.UpdateChatLines req, CancellationToken ct)
    {
        var userToChangeAccountId = Route<string>("accountId")!;
        return userService.UpdateChatLinesAsync(userToChangeAccountId, req, ct);
    }
}
