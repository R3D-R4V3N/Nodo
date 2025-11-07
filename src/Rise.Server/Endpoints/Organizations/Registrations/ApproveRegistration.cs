using Ardalis.Result;
using Rise.Services.Registrations;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Organizations.Registrations;

public class ApproveRegistration(IRegistrationService registrationService) : Endpoint<RegistrationRequest.Approve, Result>
{
    public override void Configure()
    {
        Post("/api/organizations/registrations/approve");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result> ExecuteAsync(RegistrationRequest.Approve req, CancellationToken ct)
    {
        return registrationService.ApproveRegistrationAsync(req, ct);
    }
}
