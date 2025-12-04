using Rise.Domain.Emergencies;
using Rise.Shared.Emergencies;

namespace Rise.Services.Emergencies.Mapper;
internal static class EmergencyTypeMapper
{
    public static EmergencyTypeDto ToDto(this EmergencyType category)
     => (EmergencyTypeDto)category;

    public static EmergencyType ToDomain(this EmergencyTypeDto dto)
        => (EmergencyType)dto;
}
