using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Services.Events.Mapper;
using Rise.Services.Identity;
using Rise.Shared.Events;
using Rise.Shared.Identity;

namespace Rise.Services.Events;

public class EventService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IEventService
{
    public async Task<Result<EventsResponse.GetEvents>> GetEventsAsync(CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Result.Unauthorized("U moet ingelogd zijn om events te verkrijgen.");

        var user = await dbContext.Users
            .SingleOrDefaultAsync(u => u.AccountId == userId, ctx);

        if (user is null)
            return Result.Unauthorized("Gebruiker niet gevonden.");

        var events = await dbContext.Events
            .Include(e => e.InterestedUsers)
            .OrderBy(e => e.Date)
            .ToListAsync(ctx);

        var eventDtos = events.Select(EventMapper.ToGetDto);

        return Result.Success(new EventsResponse.GetEvents
        {
            Events = eventDtos,
            TotalCount = events.Count
        });
    }

    public async Task<Result<EventResponse.ToggleInterest>> ToggleInterestAsync(int eventId, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User?.GetUserId();
        
        if (string.IsNullOrWhiteSpace(userId))
            return Result.Unauthorized("U moet ingelogd zijn om interesse te tonen.");

        var user = await dbContext.Users
            .Include(u => u.InterestedInEvents)
            .SingleOrDefaultAsync(u => u.AccountId == userId, ctx);

        if (user is null)
            return Result.Unauthorized("Gebruiker niet gevonden.");

        var eventEntity = await dbContext.Events
            .Include(e => e.InterestedUsers)
            .SingleOrDefaultAsync(e => e.Id == eventId, ctx);

        if (eventEntity is null)
            return Result.NotFound("Evenement niet gevonden.");

        bool isInterested;
        
        if (eventEntity.InterestedUsers.Any(u => u.Id == user.Id))
        {
            // User is already interested, remove them
            eventEntity.InterestedUsers.Remove(user);
            isInterested = false;
        }
        else
        {
            // User is not interested, add them
            eventEntity.InterestedUsers.Add(user);
            isInterested = true;
        }

        await dbContext.SaveChangesAsync(ctx);

        return Result.Success(new EventResponse.ToggleInterest
        {
            IsInterested = isInterested,
            InterestedCount = eventEntity.InterestedUsers.Count
        });
    }
}