using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Registrations;
using Rise.Persistence;
using Rise.Domain.Users.Properties;
using Rise.Shared.Assets;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Users;

namespace Rise.Server.Endpoints.Identity.Accounts;

/// <summary>
/// Register a new user,
/// See https://fast-endpoints.com/
/// </summary>
/// <param name="userManager"></param>
/// <param name="dbContext"></param>
/// <param name="passwordHasher"></param>
public class Register(
    UserManager<IdentityUser> userManager,
    ApplicationDbContext dbContext,
    IPasswordHasher<IdentityUser> passwordHasher) : Endpoint<AccountRequest.Register, Result>
{
    public override void Configure()
    {
        Post("/api/identity/accounts/register");
        AllowAnonymous(); // Open for all at the moment, but you can restrict it to only admins.
                          // Roles(AppRoles.Administrator);
    }

    public override async Task<Result> ExecuteAsync(AccountRequest.Register req, CancellationToken ctx)
    {
        if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
        {
            return Result.Invalid(new ValidationError(nameof(req.Email), "Ongeldige gegevens."));
        }

        var normalizedEmail = userManager.NormalizeEmail(req.Email);

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Result.Invalid(new ValidationError(nameof(req.Email), "Ongeldig e-mailadres."));
        }

        if (await userManager.FindByEmailAsync(req.Email) is not null)
        {
            return Result.Conflict("Er bestaat al een account met dit e-mailadres.");
        }

        var hasPendingRequest = await dbContext.RegistrationRequests
            .AnyAsync(r => r.NormalizedEmail == normalizedEmail && r.Status == RegistrationStatus.Pending, ctx);

        if (hasPendingRequest)
        {
            return Result.Conflict("Er is al een lopende registratieaanvraag voor dit e-mailadres.");
        }

        var organization = await dbContext.Organizations
            .SingleOrDefaultAsync(o => o.Id == req.OrganizationId, ctx);

        if (organization is null)
        {
            return Result.Invalid(new ValidationError(nameof(req.OrganizationId), "Ongeldige organisatie geselecteerd."));
        }

        var passwordUser = new IdentityUser { UserName = req.Email, Email = req.Email };
        var hashedPassword = passwordHasher.HashPassword(passwordUser, req.Password);

        var birthDate = req.BirthDate ?? DateOnly.FromDateTime(DateTime.Today.AddYears(-18));
        var gender = (GenderType)req.Gender;
        var avatarUrl = string.IsNullOrWhiteSpace(req.AvatarDataUrl)
            ? DefaultImages.GetProfile(req.Email)
            : req.AvatarDataUrl;

        var registration = RegistrationRequest.Create(
            req.Email,
            normalizedEmail,
            req.FirstName!,
            req.LastName!,
            birthDate,
            gender,
            avatarUrl,
            hashedPassword,
            organization);

        dbContext.RegistrationRequests.Add(registration);
        await dbContext.SaveChangesAsync(ctx);

        return Result.SuccessWithMessage("Je aanvraag is ingediend en wacht op goedkeuring door een begeleider.");
    }

}
