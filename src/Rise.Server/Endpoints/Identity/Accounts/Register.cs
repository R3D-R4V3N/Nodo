using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Shared.Identity.Accounts;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Register a new user,
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="userManager"></param>
/// <param name="userStore"></param>
public class Register(UserManager<IdentityUser> userManager, IUserStore<IdentityUser> userStore, ApplicationDbContext dbContext) : Endpoint<AccountRequest.Register, Result>
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
        var user = new IdentityUser();
        await userStore.SetUserNameAsync(user, req.Email, CancellationToken.None);
        await emailStore.SetEmailAsync(user, req.Email, CancellationToken.None);
        var result = await userManager.CreateAsync(user, req.Password!);

        if (!result.Succeeded)
        {
            return Result.Error(result.Errors.First().Description);
        }

        var organization = await dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == req.OrganizationId, ctx);

        if (organization is null)
        {
            await userManager.DeleteAsync(user);
            return Result.NotFound("De geselecteerde organisatie werd niet gevonden.");
        }

        var firstName = req.FirstName!.Trim();
        var lastName = req.LastName!.Trim();

        var applicationUser = new ApplicationUser(
            user.Id,
            firstName,
            lastName,
            "Nog geen bio ingevuld.",
            UserType.ChatUser,
            organization);

        dbContext.ApplicationUsers.Add(applicationUser);
        await dbContext.SaveChangesAsync(ctx);

        return Result.Success();
    }

}