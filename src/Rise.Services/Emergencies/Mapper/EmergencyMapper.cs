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
            NotifierFirstName = emergency.MadeByUser.FirstName.Value,
            NotifierLastName = emergency.MadeByUser.LastName.Value,
            NotifierFullName = $"{emergency.MadeByUser.FirstName} {emergency.MadeByUser.LastName}",
            Type = emergency.Type.ToDto(),
            ReportedAt = emergency.Range.End,
            ResolvedCount = emergency.HasResolved.Count,
            AllowedResolverCount = emergency.AllowedToResolve.Count,
        };
    }
}
