using System.Security.Claims;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Registrations;

public class GetPending(IRegistrationService registrationService) : EndpointWithoutRequest<Result<RegistrationResponse.PendingList>>
{
    public override void Configure()
    {
        Get("/api/registrations/pending");
        Claims(ClaimTypes.NameIdentifier);
        Roles(AppRoles.Supervisor);
    }

    public override Task<Result<RegistrationResponse.PendingList>> ExecuteAsync(CancellationToken ct)
        => registrationService.GetPendingAsync(ct);
}
