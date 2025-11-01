using System.Security.Claims;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Registrations;

public class GetRegistration(IRegistrationService registrationService) : Endpoint<RegistrationRequest.Get, Result<RegistrationResponse.Detail>>
{
    public override void Configure()
    {
        Get("/api/registrations/{RegistrationId:int}");
        Claims(ClaimTypes.NameIdentifier);
        Roles(AppRoles.Supervisor);
    }

    public override Task<Result<RegistrationResponse.Detail>> ExecuteAsync(RegistrationRequest.Get req, CancellationToken ct)
    {
        req.RegistrationId = Route<int>(nameof(req.RegistrationId));
        return registrationService.GetDetailAsync(req.RegistrationId, ct);
    }
}
