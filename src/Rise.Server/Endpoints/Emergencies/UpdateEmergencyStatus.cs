using Rise.Shared.Emergencies;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Emergencies;

public class UpdateEmergencyStatus(IEmergencyService emergencyService)
    : Endpoint<EmergencyRequest.UpdateStatus, Result<EmergencyResponse.UpdateStatus>>
{
    public override void Configure()
    {
        Put("/api/emergencies/{EmergencyId}/status");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<EmergencyResponse.UpdateStatus>> ExecuteAsync(
        EmergencyRequest.UpdateStatus request,
        CancellationToken ct)
    {
        request.EmergencyId = Route<int>("EmergencyId");
        return emergencyService.UpdateStatusAsync(request, ct);
    }
}
