using System.Security.Claims;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Registrations;

public class Reject(IRegistrationService registrationService) : Endpoint<RegistrationRequest.Reject, Result>
{
    public override void Configure()
    {
        Post("/api/registrations/{RegistrationId:int}/reject");
        Claims(ClaimTypes.NameIdentifier);
        Roles(AppRoles.Supervisor);
    }

    public override Task<Result> ExecuteAsync(RegistrationRequest.Reject req, CancellationToken ct)
    {
        req.RegistrationId = Route<int>(nameof(req.RegistrationId));
        return registrationService.RejectAsync(req, ct);
    }
}
