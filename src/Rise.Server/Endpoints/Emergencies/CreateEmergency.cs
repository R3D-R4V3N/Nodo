using Rise.Shared.Emergencies;
using Rise.Shared.Common;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Emergencies;

public class CreateEmergency(IEmergencyService emergencyService) : Endpoint<EmergencyRequest.CreateEmergency, Result<EmergencyResponse.Create>>
{
    public override void Configure()
    {
        Post("/api/emergencies");
        Roles(AppRoles.User);
    }

    public override Task<Result<EmergencyResponse.Create>> ExecuteAsync(EmergencyRequest.CreateEmergency emergency, CancellationToken ct)
    {
        return emergencyService.CreateEmergencyAsync(emergency, ct);
    }
}
