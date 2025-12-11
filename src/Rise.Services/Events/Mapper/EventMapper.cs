using Rise.Domain.Events;
using Rise.Shared.Events;

namespace Rise.Services.Events.Mapper;

internal static class EventMapper
{
    public static EventDto.Get ToGetDto(Event eventEntity)
    {
        return new EventDto.Get
        {
            Id = eventEntity.Id,
            Name = eventEntity.Name,
            Date = eventEntity.Date,
            Location = eventEntity.Location,
            Price = eventEntity.Price,
            ImageUrl = eventEntity.ImageUrl,
            InterestedUsers = eventEntity.InterestedUsers?.Select(u => new EventDto.InterestedUser
            {
                Id = u.Id,
                Name = $"{u.FirstName.Value} {u.LastName.Value}",
                AvatarUrl = u.AvatarUrl ?? string.Empty
            }).ToList() ?? []
        };
    }
}
