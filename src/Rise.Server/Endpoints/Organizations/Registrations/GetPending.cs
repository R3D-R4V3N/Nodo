using Ardalis.Result;
using Rise.Services.Registrations;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Organizations.Registrations;

public class GetPending(IRegistrationService registrationService) : EndpointWithoutRequest<Result<RegistrationResponse.PendingList>>
{
    public override void Configure()
    {
        Get("/api/organizations/registrations/pending");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<RegistrationResponse.PendingList>> ExecuteAsync(CancellationToken ct)
    {
        return registrationService.GetPendingRegistrationsAsync(ct);
    }
}
