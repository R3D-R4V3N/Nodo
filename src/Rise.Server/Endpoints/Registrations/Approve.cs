using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Rise.Domain.Registrations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Persistence;
using Rise.Shared.Assets;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Registrations;

public class Approve(
    ApplicationDbContext dbContext,
    UserManager<IdentityUser> userManager)
    : Endpoint<RegistrationRequest.Approve, Result>
{
    public override void Configure()
    {
        Post("/api/registrations/{id:int}/approve");
        Roles(AppRoles.Administrator, AppRoles.Supervisor);
    }

    public override async Task<Result> ExecuteAsync(RegistrationRequest.Approve req, CancellationToken ct)
    {
        var registrationId = Route<int>("id");

        var registration = await dbContext.RegistrationRequests
            .Include(r => r.Organization)
            .FirstOrDefaultAsync(r => r.Id == registrationId, ct);

        if (registration is null)
        {
            return Result.NotFound("Registratie niet gevonden.");
        }

        if (registration.Status == RegistrationRequestStatus.Approved)
        {
            return Result.Conflict("Deze registratie is al goedgekeurd.");
        }

        var supervisor = await dbContext.Supervisors
            .Include(s => s.Organization)
            .FirstOrDefaultAsync(s => s.Id == req.SupervisorId, ct);

        if (supervisor is null)
        {
            return Result.NotFound("Supervisor niet gevonden.");
        }

        if (supervisor.Organization.Id != registration.OrganizationId)
        {
            return Result.Conflict("De geselecteerde supervisor behoort niet tot dezelfde organisatie.");
        }

        var existingProfile = await dbContext.Users
            .AnyAsync(u => u.AccountId == registration.AccountId, ct);

        if (existingProfile)
        {
            return Result.Conflict("Er bestaat al een gebruikersprofiel voor dit account.");
        }

        var identityAccount = await userManager.FindByIdAsync(registration.AccountId);
        if (identityAccount is null)
        {
            return Result.NotFound("Account niet gevonden.");
        }

        var biographyResult = Biography.Create("Nog geen biografie ingevuld.");
        if (!biographyResult.IsSuccess)
        {
            return Result.Error(string.Join(' ', biographyResult.Errors));
        }

        var avatarSource = DefaultImages.GetProfile(identityAccount.Email ?? registration.AccountId);
        var avatarResult = AvatarUrl.Create(avatarSource);
        if (!avatarResult.IsSuccess)
        {
            return Result.Error(string.Join(' ', avatarResult.Errors));
        }

        var fontSizeResult = FontSize.Create(12);
        if (!fontSizeResult.IsSuccess)
        {
            return Result.Error(string.Join(' ', fontSizeResult.Errors));
        }

        var userSettings = new UserSetting()
        {
            FontSize = fontSizeResult.Value,
            IsDarkMode = false,
        };
        userSettings.AddChatTextLine("Hallo allemaal!");
        userSettings.AddChatTextLine("Leuk om hier te zijn.");

        var userProfile = new User()
        {
            AccountId = registration.AccountId,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Biography = biographyResult.Value,
            AvatarUrl = avatarResult.Value,
            BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Organization = registration.Organization,
            Supervisor = supervisor,
            UserSettings = userSettings,
        };

        registration.Approve(supervisor);
        dbContext.Users.Add(userProfile);

        var addToRoleResult = await userManager.AddToRoleAsync(identityAccount, AppRoles.User);
        if (!addToRoleResult.Succeeded)
        {
            return Result.Error(string.Join(' ', addToRoleResult.Errors.Select(e => e.Description)));
        }

        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
