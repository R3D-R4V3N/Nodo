using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Registrations;

public class Reject(
    ApplicationDbContext dbContext,
    UserManager<IdentityUser> userManager)
    : Endpoint<RegistrationRequest.Reject, Result>
{
    public override void Configure()
    {
        Post("/api/registrations/{id:int}/reject");
        Roles(AppRoles.Administrator, AppRoles.Supervisor);
    }

    public override async Task<Result> ExecuteAsync(RegistrationRequest.Reject req, CancellationToken ct)
    {
        var registrationId = Route<int>("id");

        var registration = await dbContext.RegistrationRequests
            .Include(r => r.Organization)
            .FirstOrDefaultAsync(r => r.Id == registrationId, ct);

        if (registration is null)
        {
            return Result.NotFound("Registratie niet gevonden.");
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

        registration.Reject(supervisor);

        var identityAccount = await userManager.FindByIdAsync(registration.AccountId);
        if (identityAccount is not null)
        {
            // ensure the account has no user role assigned yet
            if (await userManager.IsInRoleAsync(identityAccount, AppRoles.User))
            {
                await userManager.RemoveFromRoleAsync(identityAccount, AppRoles.User);
            }
        }

        await dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
