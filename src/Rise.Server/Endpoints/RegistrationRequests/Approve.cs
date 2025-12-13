using Ardalis.Result;
using Rise.Shared.Identity;
using Rise.Shared.RegistrationRequests;

namespace Rise.Server.Endpoints.RegistrationRequests;

public class Approve(IRegistrationRequestService registrationRequestService) : Endpoint<RegistrationRequestRequest.Approve, Result<RegistrationRequestResponse.Approve>>
{
    public override void Configure()
    {
        Post("/api/registrations/{requestId:int}/approve");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
        Summary(s =>
        {
            s.Summary = "Approve registration request";
            s.Description = "Approves a pending registration request by id.";
        });
    }

    public override Task<Result<RegistrationRequestResponse.Approve>> ExecuteAsync(RegistrationRequestRequest.Approve req, CancellationToken ct)
    {
        var requestId = Route<int>("requestId");
        return registrationRequestService.ApproveAsync(requestId, req, ct);
    }
}
