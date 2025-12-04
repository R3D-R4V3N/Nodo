using Rise.Domain.Users;
using System.Security.Claims;

namespace Rise.Tests.Shared;
public static class ServicesData
{
    public static ClaimsPrincipal GetValidClaimsPrincipal(BaseUser? user)
    {
        if (user is null)
            return new ClaimsPrincipal();

        return new ClaimsPrincipal(
            new ClaimsIdentity([new Claim(ClaimTypes.NameIdentifier, user.AccountId)])
        );
    }
}