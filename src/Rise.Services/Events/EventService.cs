using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Events;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Events.Mapper;
using Rise.Services.Identity;
using Rise.Shared.Events;
using Rise.Shared.Identity;
using EventResponse = Rise.Shared.Events.EventResponse;

namespace Rise.Services.Events;

public class EventService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IEventService
{
    
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    
    public async Task<Result<EventResponse.GetEvents>> GetEventsAsync(CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(userId))
            return Result.Unauthorized("U moet ingelogd zijn om events te verkrijgen.");
        
        var userExists = await dbContext.Users.AnyAsync(u => u.AccountId == userId, ctx);
        
        if (!userExists)
            userExists = await dbContext.Admins.AnyAsync(s => s.AccountId == userId, ctx);
        if (!userExists)
            return Result.Unauthorized("Gebruiker niet gevonden.");

        var events = await dbContext.Events
            .Where(e => !e.IsDeleted)
            .Include(e => e.InterestedUsers)
            .OrderBy(e => e.Date)
            .ToListAsync(ctx);

        var eventDtos = events.Select(EventMapper.ToGetDto);

        return Result.Success(new EventResponse.GetEvents
        {
            Events = eventDtos,
            TotalCount = events.Count
        });
    }

    public async Task<Result<EventResponse.AddEventResponse>> AddEvent(EventRequest.AddEventRequest request, CancellationToken ctx = default)
    {
        var principal = _sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var isAuthorized = principal.IsInRole(AppRoles.Administrator) || principal.IsInRole(AppRoles.Supervisor);
        if (!isAuthorized)
        {
            return Result.Forbidden();
        }

        if (request is null ||
            string.IsNullOrWhiteSpace(request.Name) ||
            string.IsNullOrWhiteSpace(request.Location) ||
            request.Date == default)
        {
            return Result.Invalid(new ValidationError(nameof(request), "Ongeldige gegevens voor het evenement."));
        }

        var eventEntity = new Event
        {
            Name = request.Name,
            Date = request.Date,
            Location = request.Location,
            Price = request.Price,
            ImageUrl = string.IsNullOrWhiteSpace(request.ImageUrl) ? string.Empty : request.ImageUrl,
            InterestedUsers = new List<User>()
        };

        _dbContext.Events.Add(eventEntity);
        await _dbContext.SaveChangesAsync(ctx);

        return Result.Success(new EventResponse.AddEventResponse
        {
            Message = "Evenement aangemaakt."
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
            eventEntity.InterestedUsers.Remove(user);
            isInterested = false;
        }
        else
        {
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

    public async Task<Result<EventResponse.DeleteEventResponse>> DeleteEvent(EventRequest.DeleteEventRequest req, CancellationToken ct)
    {
        var eventEntity = await _dbContext.Events
            .FirstOrDefaultAsync(e => e.Id == req.EventId, ct);

        if (eventEntity is null)
            return Result.NotFound("Evenement niet gevonden.");

        _dbContext.Events.Remove(eventEntity);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success(new EventResponse.DeleteEventResponse
        {
            Message = "Evenement succesvol verwijderd."
        });
    }
}
