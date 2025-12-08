using Rise.Shared.Emergencies;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Emergencies;

public class UpdateEmergencyStatus(IEmergencyService emergencyService)
    : Endpoint<EmergencyRequest.Resolve, Result<EmergencyResponse.Resolve>>
{
    public override void Configure()
    {
        Put("/api/emergencies/{EmergencyId}/status");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<EmergencyResponse.Resolve>> ExecuteAsync(
        EmergencyRequest.Resolve request,
        CancellationToken ct)
    {
        request.EmergencyId = Route<int>("EmergencyId");
        return emergencyService.ResolveAsync(request, ct);
    }
}
