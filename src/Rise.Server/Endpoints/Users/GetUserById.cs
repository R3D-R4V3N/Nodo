using System.Security.Claims;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Users;

public class GetUserById(IUserService userService) : Endpoint<string, Result<UserResponse.CurrentUser>>
{
    public override void Configure()
    {
        Get("/api/users/{accountId}");
        AllowAnonymous();
    }

    public override Task<Result<UserResponse.CurrentUser>> ExecuteAsync(string accountId, CancellationToken ct)
    {
        var resolvedAccountId = string.IsNullOrWhiteSpace(accountId)
            ? Route<string>("accountId")
            : accountId;

        if (string.IsNullOrWhiteSpace(resolvedAccountId))
        {
            return Task.FromResult(Result.Error<UserResponse.CurrentUser>("Ongeldig account ID."));
        }

        return userService.GetUserAsync(resolvedAccountId, ct);
    }
}