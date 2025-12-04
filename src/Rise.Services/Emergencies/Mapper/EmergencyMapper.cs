using Rise.Domain.Emergencies;
using Rise.Shared.Emergencies;

namespace Rise.Services.Emergencies.Mapper;
public static class EmergencyMapper
{
    public static EmergencyDto.Get ToGetDto(this Emergency emergency)
    {
        return new EmergencyDto.Get
        {
            Id = emergency.Id,
            Type = emergency.Type.ToDto(),
            Status = emergency.Status.ToDto(),
            ChatId = emergency.HappenedInChat.Id,
            CreatedAt = emergency.Range.End,
            Reporter = emergency.MadeByUser.ToString(),
        };
    }
}
