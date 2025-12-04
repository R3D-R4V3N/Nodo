using Rise.Domain.Emergencies;
using Rise.Shared.Emergencies;

namespace Rise.Services.Emergencies.Mapper;

internal static class EmergencyStatusMapper
{
    public static EmergencyStatusDto ToDto(this EmergencyStatus status)
        => (EmergencyStatusDto)status;

    public static EmergencyStatus ToDomain(this EmergencyStatusDto status)
        => (EmergencyStatus)status;
}
