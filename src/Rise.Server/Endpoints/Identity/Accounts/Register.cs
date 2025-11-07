using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Organizations;
using Rise.Persistence;
using Rise.Shared.Identity.Accounts;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Register a new user,
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="userManager"></param>
/// <param name="userStore"></param>
/// <param name="dbContext"></param>
public class Register(
    UserManager<IdentityUser> userManager,
    IUserStore<IdentityUser> userStore,
    ApplicationDbContext dbContext) : Endpoint<AccountRequest.Register, Result>
{
    public override void Configure()
    {
        Post("/api/identity/accounts/register");
        AllowAnonymous(); // Open for all at the moment, but you can restrict it to only admins.
                          // Roles(AppRoles.Administrator);
    }

    public override async Task<Result> ExecuteAsync(AccountRequest.Register req, CancellationToken ctx)
    {
        if (!userManager.SupportsUserEmail)
        {
            return Result.CriticalError("Requires a user store with email support.");
        }
        var emailStore = (IUserEmailStore<IdentityUser>)userStore;
        var organization = await dbContext
            .Organizations
            .SingleOrDefaultAsync(o => o.Id == req.OrganizationId, ctx);

        if (organization is null)
        {
            return Result.Invalid(new ValidationError(nameof(req.OrganizationId), "Ongeldige organisatie geselecteerd."));
        }

        var user = new IdentityUser();
        await userStore.SetUserNameAsync(user, req.Email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, req.Email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, req.Password!);

        if (!result.Succeeded)
        {
            return Result.Error(result.Errors.First().Description);
        }

        try
        {
            var registration = new UserRegistration
            {
                AccountId = user.Id,
                Email = req.Email!.Trim(),
                FirstName = req.FirstName!.Trim(),
                LastName = req.LastName!.Trim(),
                OrganizationId = organization.Id,
            };

            dbContext.UserRegistrations.Add(registration);
            await dbContext.SaveChangesAsync(ctx);
        }
        catch
        {
            await userManager.DeleteAsync(user);
            throw;
        }

        return Result.Success();
    }

}