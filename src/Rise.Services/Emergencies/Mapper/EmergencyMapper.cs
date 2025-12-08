using Rise.Domain.Emergencies;
using Rise.Services.Chats.Mapper;
using Rise.Shared.Emergencies;

namespace Rise.Services.Emergencies.Mapper;
public static class EmergencyMapper
{
    public static EmergencyDto.GetEmergencies ToGetEmergenciesDto(this Emergency emergency)
    {
        return new EmergencyDto.GetEmergencies
        {
            Id = emergency.Id,
            Type = emergency.Type.ToDto(),
            Status = emergency.Status.ToDto(),
            ChatId = emergency.HappenedInChat.Id,
            CreatedAt = emergency.Range.End,
            Reporter = emergency.MadeByUser.ToString(),
        };
    }
    public static EmergencyDto.GetEmergency ToGetEmergencyDto(this Emergency emergency)
    {
        return new EmergencyDto.GetEmergency
        {
            Id = emergency.Id,
            Type = emergency.Type.ToDto(),
            Status = emergency.Status.ToDto(),
            Chat = emergency.HappenedInChat.ToEmergencyDto(),
            CreatedAt = emergency.Range.End,
            Reporter = emergency.MadeByUser.ToString(),
        };
    }
}
