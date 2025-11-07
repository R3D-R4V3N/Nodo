using Ardalis.Result;
using Rise.Services.Registrations;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Server.Endpoints.RegistrationRequests;

public class ListPending(IRegistrationRequestService registrationRequestService)
    : EndpointWithoutRequest<Result<IReadOnlyCollection<RegistrationRequestDto.ListItem>>>
{
    public override void Configure()
    {
        Get("/api/registration-requests/pending");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<IReadOnlyCollection<RegistrationRequestDto.ListItem>>> ExecuteAsync(CancellationToken ct)
    {
        return registrationRequestService.GetPendingRequestsForSupervisorAsync(ct);
    }
}
