using Ardalis.Result;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.RegistrationRequests;

public class Approve(IRegistrationRequestService registrationRequestService)
    : Endpoint<RegistrationRequestRequest.Approve, Result>
{
    public override void Configure()
    {
        Post("/api/registration-requests/{requestId:int}/approve");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result> ExecuteAsync(RegistrationRequestRequest.Approve req, CancellationToken ct)
    {
        var requestId = Route<int>("requestId");
        return registrationRequestService.ApproveAsync(requestId, req, ct);
    }
}
