using Rise.Shared.Emergencies;
using Rise.Shared.Common;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Emergencies;

public class GetEmergencies(IEmergencyService emergencyService) : Endpoint<QueryRequest.SkipTake, Result<EmergencyResponse.GetEmergencies>>
{
    public override void Configure()
    {
        Get("/api/emergencies");
        Roles(AppRoles.Supervisor);
    }

    public override Task<Result<EmergencyResponse.GetEmergencies>> ExecuteAsync(QueryRequest.SkipTake request, CancellationToken ct)
    {
        return emergencyService.GetEmergenciesAsync(request, ct);
    }
}
