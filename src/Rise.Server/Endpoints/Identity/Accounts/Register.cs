using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Rise.Domain.Registrations;
using Rise.Domain.Users.Properties;
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
    ApplicationDbContext dbContext)
    : Endpoint<AccountRequest.Register, Result>
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

        var firstNameResult = FirstName.Create(req.FirstName);
        var lastNameResult = LastName.Create(req.LastName);

        if (!firstNameResult.IsSuccess || !lastNameResult.IsSuccess)
        {
            await userManager.DeleteAsync(user);
            var errors = firstNameResult.Errors.Concat(lastNameResult.Errors)
                .Where(e => !string.IsNullOrWhiteSpace(e))
                .ToArray();
            var errorMessage = errors.Length > 0
                ? string.Join(" ", errors)
                : "Onbekende fout bij het verwerken van profielgegevens.";
            return Result.Error(errorMessage);
        }

        var organization = await dbContext.Organizations
            .FirstOrDefaultAsync(o => o.Id == req.OrganizationId, ctx);

        if (organization is null)
        {
            await userManager.DeleteAsync(user);
            return Result.NotFound("Organisatie niet gevonden.");
        }

        var registration = new RegistrationRequest
        {
            AccountId = user.Id,
            FirstName = firstNameResult.Value,
            LastName = lastNameResult.Value,
            Organization = organization,
            OrganizationId = organization.Id
        };

        dbContext.RegistrationRequests.Add(registration);
        await dbContext.SaveChangesAsync(ctx);

        return Result.Success();
    }

}
