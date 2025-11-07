using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Rise.Domain.Users.Registrations;
using Rise.Persistence;
using Rise.Services.Organizations;
using Rise.Shared.Identity.Accounts;
using System.Linq;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Register a new user,
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="userManager"></param>
/// <param name="userStore"></param>
public class Register(
    UserManager<IdentityUser> userManager,
    IUserStore<IdentityUser> userStore,
    IOrganizationService organizationService,
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
        if (req.OrganizationId is null)
        {
            return Result.Invalid(new ValidationError(nameof(req.OrganizationId), "Selecteer een organisatie."));
        }

        if (!userManager.SupportsUserEmail)
        {
            return Result.CriticalError("Requires a user store with email support.");
        }
        var emailStore = (IUserEmailStore<IdentityUser>)userStore;
        var user = new IdentityUser();

        var trimmedEmail = req.Email!.Trim();
        await userStore.SetUserNameAsync(user, trimmedEmail, CancellationToken.None);
        await emailStore.SetEmailAsync(user, trimmedEmail, CancellationToken.None);

        var result = await userManager.CreateAsync(user, req.Password!);

        if (!result.Succeeded)
        {
            return Result.Error(result.Errors.First().Description);
        }

        var organization = await organizationService.FindByIdAsync(req.OrganizationId.Value, ctx);

        if (organization is null)
        {
            await userManager.DeleteAsync(user);
            return Result.Invalid(new ValidationError(nameof(req.OrganizationId), "De gekozen organisatie bestaat niet meer."));
        }

        var trimmedFullName = req.FullName?.Trim();

        if (string.IsNullOrWhiteSpace(trimmedFullName))
        {
            await userManager.DeleteAsync(user);
            return Result.Invalid(new ValidationError(nameof(req.FullName), "Naam mag niet leeg zijn."));
        }

        var registrationResult = UserRegistrationRequest.Create(
            user.Id,
            trimmedEmail,
            organization,
            trimmedFullName);

        if (!registrationResult.IsSuccess)
        {
            await userManager.DeleteAsync(user);

            if (registrationResult.ValidationErrors.Any())
            {
                return Result.Invalid(registrationResult.ValidationErrors);
            }

            return Result.Error(registrationResult.Errors.FirstOrDefault() ?? "Kon registratieaanvraag niet opslaan.");
        }

        dbContext.UserRegistrationRequests.Add(registrationResult.Value);
        await dbContext.SaveChangesAsync(ctx);

        // You can do more stuff when injecting a DbContext and create user stuff for example:
        // dbContext.Technicians.Add(new Technician("Fname", "Lname", user.Id));
        // or assinging a specific role etc using the RoleManager<IdentityUser> (inject it in the primary constructor).

        
        // You can send a confirmation email by using a SMTP server or anything in the like. 
        // await SendConfirmationEmailAsync(user, userManager, context, email); or do something that matters

        return Result.Success("Registratieaanvraag is ontvangen en wacht op goedkeuring.");
    }

}