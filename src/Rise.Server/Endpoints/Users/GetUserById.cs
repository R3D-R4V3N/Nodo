using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class GetUserById(IUserService userService) : EndpointWithoutRequest<Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Get("/api/users/{accountId}");
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "Get user profile";
            s.Description = "Retrieves public profile information for the specified account id.";
        });
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(CancellationToken ct)
    {
        var accountIdToChange = Route<string>("accountId")!;   
        return userService.GetUserAsync(accountIdToChange, ct);
    }
}