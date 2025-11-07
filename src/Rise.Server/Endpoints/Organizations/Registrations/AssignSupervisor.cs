using Ardalis.Result;
using Rise.Services.Registrations;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Organizations.Registrations;

public class AssignSupervisor(IRegistrationService registrationService) : Endpoint<RegistrationRequest.AssignSupervisor, Result<RegistrationResponse.Updated>>
{
    public override void Configure()
    {
        Post("/api/organizations/registrations/assign");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<RegistrationResponse.Updated>> ExecuteAsync(RegistrationRequest.AssignSupervisor req, CancellationToken ct)
    {
        return registrationService.AssignSupervisorAsync(req, ct);
    }
}
