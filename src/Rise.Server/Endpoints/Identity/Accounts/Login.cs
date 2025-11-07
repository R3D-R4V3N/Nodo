using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Organizations;
using Rise.Persistence;
using Rise.Shared.Identity;
using Rise.Shared.Identity.Accounts;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Login Endpoint.
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="signInManager"></param>
/// <param name="userManager"></param>
/// <param name="dbContext"></param>
public class Login(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    ApplicationDbContext dbContext) : Endpoint<AccountRequest.Login, Result>
{
    private const bool UseCookies = true;
    private const bool UseSessionCookies = true;
    public override void Configure()
    {
        Post("/api/identity/accounts/login");
        AllowAnonymous();
    }

    public override async Task<Result> ExecuteAsync(AccountRequest.Login req, CancellationToken ctx)
    {
        var useCookieScheme = UseCookies || UseSessionCookies;
        var isPersistent = UseCookies && (UseSessionCookies != true);
        signInManager.AuthenticationScheme = useCookieScheme ? IdentityConstants.ApplicationScheme : IdentityConstants.BearerScheme;

        var identityUser = await userManager.FindByEmailAsync(req.Email!);
        if (identityUser is null)
        {
            return Result.Unauthorized("Ongeldige combinatie van e-mailadres en wachtwoord.");
        }

        var result = await signInManager.PasswordSignInAsync(identityUser.UserName!, req.Password!, isPersistent, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(req.TwoFactorCode))
            {
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(req.TwoFactorCode, isPersistent, rememberClient: isPersistent);
            }
            else if (!string.IsNullOrEmpty(req.TwoFactorRecoveryCode))
            {
                result = await signInManager.TwoFactorRecoveryCodeSignInAsync(req.TwoFactorRecoveryCode);
            }
        }

        if (!result.Succeeded)
        {
            return Result.Unauthorized(result.ToString());
        }

        var registration = await dbContext.UserRegistrations
            .AsNoTracking()
            .SingleOrDefaultAsync(r => r.AccountId == identityUser.Id, ctx);

        if (registration is not null && registration.Status != RegistrationStatus.Approved)
        {
            await signInManager.SignOutAsync();
            return Result.Unauthorized("Je registratie wacht nog op goedkeuring door een begeleider.");
        }

        var roles = await userManager.GetRolesAsync(identityUser);
        if (!roles.Contains(AppRoles.Administrator) && !roles.Contains(AppRoles.Supervisor) && !roles.Contains(AppRoles.User))
        {
            await signInManager.SignOutAsync();
            return Result.Unauthorized("Je account is nog niet geactiveerd.");
        }

        // The signInManager already produced the needed response in the form of a cookie or bearer token.

        return Result.Success();
    }
}