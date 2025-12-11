using Microsoft.EntityFrameworkCore;
using Rise.Domain.Emergencies;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Persistence;
using Rise.Services.Emergencies.Mapper;
using Rise.Services.Identity;
using Rise.Shared.Common;
using Rise.Shared.Emergencies;
using Rise.Shared.Identity;

namespace Rise.Services.Emergencies;

public class EmergencyService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IEmergencyService
{
    public async Task<Result<EmergencyResponse.Create>> CreateEmergencyAsync(EmergencyRequest.CreateEmergency request, CancellationToken ctx = default)
    {
        var principal = sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var sender = await dbContext
            .Users
            .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (sender is null)
        {
            return Result.Unauthorized("De huidige gebruiker heeft geen geldig profiel.");
        }

        var chat = await dbContext
            .Chats
            .Include(c => c.Users)
            .ThenInclude(u => (u as User).Supervisor)
            .Include(c => c.Messages.OrderByDescending(m => m.CreatedAt).Take(1))
            .SingleOrDefaultAsync(c => c.Id == request.ChatId, ctx);

        if (chat is null)
        {
            return Result.Unauthorized("Chat werd niet gevonden.");
        }

        if (!chat.Users.Any(u => u == sender))
        {
            return Result.Unauthorized("Geen toegang tot deze chat.");
        }

        var fiveMinutesAgo = DateTime.UtcNow.AddMinutes(-5);

        var recentEmergencyExists = await dbContext
            .Emergencies
            .AnyAsync(e => e.HappenedInChat.Id == chat.Id
                && e.MadeByUser.AccountId == accountId
                && e.Status != EmergencyStatus.Closed
                && e.CreatedAt >= fiveMinutesAgo
                , ctx);

        if (recentEmergencyExists)
        {
            return Result.Conflict("Er werd al recent een noodmelding gestuurd.");
        }

        var relatedMessage = chat.Messages.FirstOrDefault(m => m.Id == request.MessageId);

        if (relatedMessage is null)
        {
            return Result.Conflict("Bericht hoort niet bij deze chat.");
        }

        var createEmergencyResult = chat.CreateEmergency(
            sender,
            relatedMessage,
            request.Type.ToDomain()
        );

        if (!createEmergencyResult.IsSuccess)
        {
            return Result.Conflict(createEmergencyResult.Errors.ToArray());
        }

        await dbContext.SaveChangesAsync(ctx);

        return Result.Success(
            new EmergencyResponse.Create
            {
                Id = createEmergencyResult.Value.Id
            }
        );
    }

    public async Task<Result<EmergencyResponse.GetEmergencies>> GetEmergenciesAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var principal = sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await dbContext
                .Supervisors
                .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (supervisor is null)
        {
            return Result.Unauthorized();
        }

        var emergencyQuery = dbContext
            .Emergencies
            .Include(e => e.HappenedInChat)
            .Include(e => e.AllowedToResolve)
            .Include(e => e.HasResolved)
            .Include(e => e.MadeByUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            emergencyQuery = emergencyQuery.Where(c =>
                c.MadeByUser.FirstName.Value.Contains(request.SearchTerm) ||
                c.MadeByUser.LastName.Value.Contains(request.SearchTerm));
        }

        var totalCount = await emergencyQuery.CountAsync(ctx);

        var emergencies = emergencyQuery
            .AsEnumerable()
            .OrderBy(c => c.Status)
            .ThenByDescending(c => c.CreatedAt)
            .ThenBy(e => e.MadeByUser.FirstName.Value)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return Result.Success(
            new EmergencyResponse.GetEmergencies
            {
                Emergencies = emergencies.Select(EmergencyMapper.ToGetEmergenciesDto),
                TotalCount = totalCount,
            }
        );
    }

    public async Task<Result<EmergencyResponse.GetEmergency>> GetEmergencyAsync(int id, CancellationToken ctx = default)
    {
        var principal = sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await dbContext
                .Supervisors
                .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (supervisor is null)
        {
            return Result.Unauthorized();
        }

        var emergencyRange = await dbContext.Emergencies
            .AsNoTracking()
            .Where(e => e.Id == id)
            .Select(e => new { e.Id, e.Range.Start, e.Range.End })
            .FirstOrDefaultAsync(ctx);

        if (emergencyRange == null)
        {
            return Result.Conflict("Noodmelding werd niet gevondenn.");
        }

        var emergency = await dbContext
            .Emergencies
            .Include(e => e.HappenedInChat)
            .ThenInclude(c => c.Users)
            .Include(e => e.HappenedInChat.Messages
                .Where(m => m.CreatedAt >= emergencyRange.Start
                 && m.CreatedAt <= emergencyRange.End))
            .Include(e => e.AllowedToResolve)
            .Include(e => e.HasResolved)
            .Include(e => e.MadeByUser)
            .FirstOrDefaultAsync(e => e.Id == id, ctx);

        if (emergency is null)
        {
            return Result.Conflict("Noodmelding werd niet gevondenn.");
        }

        return Result.Success(
            new EmergencyResponse.GetEmergency
            {
                Emergency = emergency.ToGetEmergencyDto()
            }
        );
    }

    public async Task<Result<EmergencyResponse.Resolve>> ResolveAsync(
        EmergencyRequest.Resolve request,
        CancellationToken ctx = default)
    {
        var principal = sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await dbContext
                .Supervisors
                .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (supervisor is null)
        {
            return Result.Unauthorized();
        }

        var emergency = await dbContext
            .Emergencies
            .Include(e => e.HappenedInChat)
            .Include(e => e.AllowedToResolve)
            .Include(e => e.HasResolved)
            .Include(e => e.MadeByUser)
            .FirstOrDefaultAsync(e => e.Id == request.EmergencyId, ctx);

        if (emergency is null)
        {
            return Result.NotFound("Noodmelding werd niet gevonden.");
        }

        if (!emergency.AllowedToResolve.Contains(supervisor!))
        {
            return Result.Unauthorized("Geen toegang tot deze noodmelding.");
        }

        var updateResult = emergency.Resolve(supervisor);

        if (!updateResult.IsSuccess)
        {
            return Result.Conflict(updateResult.Errors.ToArray());
        }

        await dbContext.SaveChangesAsync(ctx);

        return Result.Success(new EmergencyResponse.Resolve
        {
            Emergency = emergency.ToGetEmergenciesDto()
        });
    }
}
