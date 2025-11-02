using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Registrations;
using Rise.Persistence;
using Rise.Shared.Identity.Accounts;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Login Endpoint.
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="signInManager"></param>
/// <param name="dbContext"></param>
public class Login(SignInManager<IdentityUser> signInManager, ApplicationDbContext dbContext)
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
        signInManager.AuthenticationScheme = useCookieScheme
            ? IdentityConstants.ApplicationScheme
            : IdentityConstants.BearerScheme;

        var result = await signInManager.PasswordSignInAsync(req.Email!, req.Password!, isPersistent, lockoutOnFailure: true);

        if (result.RequiresTwoFactor)
        {
            if (!string.IsNullOrEmpty(req.TwoFactorCode))
            {
                result = await signInManager.TwoFactorAuthenticatorSignInAsync(
                    req.TwoFactorCode,
                    isPersistent,
                    rememberClient: isPersistent);
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

        var identityUser = await signInManager.UserManager.FindByNameAsync(req.Email!);

        if (identityUser is null)
        {
            await signInManager.SignOutAsync();
            return Result.Unauthorized("Ongeldige inloggegevens.");
        }

        var hasProfile = await dbContext.Users
            .AsNoTracking()
            .AnyAsync(u => u.AccountId == identityUser.Id, ctx);

        if (!hasProfile)
        {
            var registration = await dbContext.RegistrationRequests
                .AsNoTracking()
                .SingleOrDefaultAsync(r => r.AccountId == identityUser.Id, ctx);

            await signInManager.SignOutAsync();

            if (registration is null)
            {
                return Result.Unauthorized("Account is niet actief.");
            }

            return registration.Status switch
            {
                RegistrationRequestStatus.Pending => Result.Unauthorized("Je account wacht op goedkeuring door een supervisor."),
                RegistrationRequestStatus.Rejected => Result.Unauthorized("Je registratieaanvraag werd geweigerd."),
                _ => Result.Unauthorized("Account is niet actief."),
            };
        }

        // The signInManager already produced the needed response in the form of a cookie or bearer token.

        return Result.Success();
    }
}
