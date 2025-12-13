using Ardalis.Result;
using Rise.Shared.Identity;
using Rise.Shared.RegistrationRequests;

namespace Rise.Server.Endpoints.RegistrationRequests;

public class GetPending(IRegistrationRequestService registrationRequestService) : EndpointWithoutRequest<Result<RegistrationRequestResponse.PendingList>>
{
    public override void Configure()
    {
        Get("/api/registrations/pending");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
        Summary(s =>
        {
            s.Summary = "List pending registrations";
            s.Description = "Returns registration requests that are awaiting approval.";
        });
    }

    public override Task<Result<RegistrationRequestResponse.PendingList>> ExecuteAsync(CancellationToken ct)
    {
        return registrationRequestService.GetPendingAsync(ct);
    }
}
