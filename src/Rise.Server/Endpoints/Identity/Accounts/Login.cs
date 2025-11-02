using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Rise.Domain.Registrations;
using Rise.Persistence;
using Rise.Shared.Identity;
using Rise.Shared.Identity.Accounts;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Login Endpoint.
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="signInManager"></param>
public class Login(
    SignInManager<IdentityUser> signInManager,
    UserManager<IdentityUser> userManager,
    ApplicationDbContext dbContext)
    : Endpoint<AccountRequest.Login, Result>
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

        var result = await signInManager.PasswordSignInAsync(req.Email!, req.Password!, isPersistent, lockoutOnFailure: true);

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

        var identityAccount = await userManager.FindByEmailAsync(req.Email!);
        if (identityAccount is null)
        {
            await signInManager.SignOutAsync();
            return Result.Unauthorized("Onbekende aanmeldgegevens.");
        }

        var roles = await userManager.GetRolesAsync(identityAccount);
        if (roles.Contains(AppRoles.Administrator) || roles.Contains(AppRoles.Supervisor))
        {
            return Result.Success();
        }

        var hasProfile = await dbContext.Users
            .AnyAsync(u => u.AccountId == identityAccount.Id, ctx);

        if (hasProfile)
        {
            return Result.Success();
        }

        var registration = await dbContext.RegistrationRequests
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.AccountId == identityAccount.Id, ctx);

        if (registration is null || registration.Status != RegistrationRequestStatus.Approved)
        {
            await signInManager.SignOutAsync();
            return Result.Unauthorized("Je account moet nog worden goedgekeurd door een supervisor.");
        }

        return Result.Success();
    }
}