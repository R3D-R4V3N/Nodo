using System.Collections.Generic;
using Ardalis.Result;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.RegistrationRequests;

public class ListPending(IRegistrationRequestService registrationRequestService)
    : EndpointWithoutRequest<Result<List<RegistrationRequestResponse.PendingItem>>>
{
    public override void Configure()
    {
        Get("/api/registration-requests");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<List<RegistrationRequestResponse.PendingItem>>> ExecuteAsync(CancellationToken ct)
    {
        return registrationRequestService.GetPendingAsync(ct);
    }
}
