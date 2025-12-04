using Rise.Shared.Emergencies;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using static FastEndpoints.Ep;

namespace Rise.Server.Endpoints.Emergencies;

public class GetEmergency(IEmergencyService emergencyService) : EndpointWithoutRequest<Result<EmergencyResponse.GetEmergency>>
{
    public override void Configure()
    {
        Get("/api/emergencies/{ChatId:int}");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<EmergencyResponse.GetEmergency>> ExecuteAsync(CancellationToken ct)
    {
        int id = Route<int>("ChatId");
        return emergencyService.GetEmergencyAsync(id, ct);
    }
}
