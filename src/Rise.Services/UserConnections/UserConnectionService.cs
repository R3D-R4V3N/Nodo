
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users.Connections;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.UserConnections.Mapper;
using Rise.Services.Users.Mapper;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace Rise.Services.UserConnections;
/// <summary>
/// Service for user connections. Note the use of <see cref="ISessionContextProvider"/> to get the current user in this layer of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class UserConnectionService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.GetFriends>> GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.ApplicationUsers
            .SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("You are not authorized to fetch user connections.");

        var query = dbContext.ApplicationUsers
            .Where(u => u.AccountId == userId)
            .SelectMany(u => u.Connections)
            .Where(c =>
                c.ConnectionType.Equals(UserConnectionType.Friend)
                || c.ConnectionType.Equals(UserConnectionType.RequestIncoming)
                || c.ConnectionType.Equals(UserConnectionType.RequestOutgoing));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p => 
                p.Connection.FirstName.Contains(request.SearchTerm) 
                || p.Connection.LastName.Contains(request.SearchTerm));
        }

        var totalCount = await query.CountAsync(ctx);

        // Apply ordering
        if (!string.IsNullOrWhiteSpace(request.OrderBy))
        {
            query = request.OrderDescending
                ? query.OrderByDescending(e => EF.Property<object>(e, request.OrderBy))
                : query.OrderBy(e => EF.Property<object>(e, request.OrderBy));
        }
        else
        {
            // Default order
            query = query
                .OrderByDescending(p => p.Connection.CreatedAt)
                .ThenBy(p => p.Connection.FirstName);
        }

        var connections = await query.AsNoTracking()
            .Skip(request.Skip)
            .Take(request.Take)
            .Include(x => x.Connection)
            .ToListAsync(ctx);

        return Result.Success(new UserConnectionResponse.GetFriends
        {
            Connections = connections.Select(UserConnectionMapper.ToGetDto),
            TotalCount = totalCount
        });
    }

    
    public async Task<Result<string>> AddFriendAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext.ApplicationUsers
            .Include(u => u.Connections)
            .ThenInclude(uc => uc.Connection) // <-- dit laadt de ApplicationUser waar UserConnection naar verwijst
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);
        // Doelgebruiker ophalen
        var targetUser = await dbContext.ApplicationUsers
            .Include(u => u.Connections)
            .ThenInclude(uc => uc.Connection)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);
        
        if (currentUser is null || targetUser is null)
            return Result.NotFound("User not found.");

        // Domeinlogica hergebruiken
        var result = currentUser.AddFriend(targetUser);

        if (!result.IsSuccess)
            return result; // bijv. conflict of invalid state

        await dbContext.SaveChangesAsync(ctx);
        return result;
    }
    
    public async Task<Result<string>> AcceptFriendAsync(string requesterAccountId, CancellationToken ct = default)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext.ApplicationUsers
            .Include(u => u.Connections)
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ct);

        // Aanvrager ophalen
        var requesterUser = await dbContext.ApplicationUsers
            .Include(u => u.Connections)
            .SingleOrDefaultAsync(u => u.AccountId == requesterAccountId, ct);

        if (currentUser is null || requesterUser is null)
            return Result.NotFound("User not found.");

        // Domeinlogica hergebruiken
        var result = currentUser.AcceptFriendRequest(requesterUser);

        if (!result.IsSuccess)
            return result; // bijv. conflict of invalid state

        await dbContext.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>> GetSuggestedFriendsAsync(QueryRequest.SkipTake request, CancellationToken ctx)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.ApplicationUsers
            .SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);

        if (loggedInUser is null)
        { 
            return Result.Unauthorized("You are not authorized to fetch user connections.");
        }
        
        var existingConnectionIds = dbContext.ApplicationUsers
                .Where(u => u.AccountId == userId)
                .SelectMany(u => u.Connections)
                .Where(c =>
                    c.ConnectionType.Equals(UserConnectionType.Friend)
                    || c.ConnectionType.Equals(UserConnectionType.RequestIncoming)
                    || c.ConnectionType.Equals(UserConnectionType.RequestOutgoing))
                .Select(c => c.Connection.AccountId)
                .ToList();
        
        var query = dbContext.ApplicationUsers
            .Where(u => u.AccountId != userId && !existingConnectionIds.Contains(u.AccountId));

        var totalCount = await query.CountAsync(ctx);

        var suggestedFriends = await query
            .Skip(request.Skip)
            .Take(request.Take)
            .ToListAsync(ctx);

        return Result.Success(new UserConnectionResponse.GetSuggestions
        {
            Users = suggestedFriends.Select(UserConnectionMapper.ToGetDto),
            TotalCount = totalCount
        });
    }
}

