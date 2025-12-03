using Rise.Shared.Common;

namespace Rise.Shared.Emergencies;

public interface IEmergencyService
{
    Task<Result<EmergencyResponse.Create>> CreateEmergencyAsync(EmergencyRequest.CreateEmergency request, CancellationToken ctx = default);
    Task<Result<EmergencyResponse.GetEmergencies>> GetEmergenciesAsync(QueryRequest.SkipTake request, CancellationToken ctx = default);
    Task<Result<EmergencyResponse.GetEmergency>> GetEmergencyAsync(int id, CancellationToken ctx = default);
}
