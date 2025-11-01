using System.Security.Claims;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Registrations;

public class Approve(IRegistrationService registrationService) : Endpoint<RegistrationRequest.Approve, Result>
{
    public override void Configure()
    {
        Post("/api/registrations/{RegistrationId:int}/approve");
        Claims(ClaimTypes.NameIdentifier);
        Roles(AppRoles.Supervisor);
    }

    public override Task<Result> ExecuteAsync(RegistrationRequest.Approve req, CancellationToken ct)
    {
        req.RegistrationId = Route<int>(nameof(req.RegistrationId));
        return registrationService.ApproveAsync(req, ct);
    }
}
