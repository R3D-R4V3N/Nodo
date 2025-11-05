using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users.Connections;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.UserConnections.Mapper;
using Rise.Shared.Common;
using Rise.Shared.Identity;
using Rise.Shared.UserConnections;

namespace Rise.Services.UserConnections;
/// <summary>
/// Service for user connections. Note the use of <see cref="ISessionContextProvider"/> to get the current user in this layer of the application.
/// </summary>
/// <param name="dbContext"></param>
/// <param name="sessionContextProvider"></param>
public class UserConnectionService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IUserConnectionService
{
    public async Task<Result<UserConnectionResponse.GetFriends>>
        GetFriendIndexAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext
            .Users
            .SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("You are not authorized to fetch user connections.");

        var query = dbContext
            .Users
            .Where(u => u.AccountId == userId)
            .SelectMany(u => u.Connections)
            .Where(c =>
                c.ConnectionType.Equals(UserConnectionType.Friend)
                || c.ConnectionType.Equals(UserConnectionType.RequestIncoming)
                || c.ConnectionType.Equals(UserConnectionType.RequestOutgoing));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            query = query.Where(p =>
                p.To.FirstName.Value.Contains(request.SearchTerm)
                || p.To.LastName.Value.Contains(request.SearchTerm));
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
                .OrderByDescending(p => p.CreatedAt)
                .ThenBy(p => p.To.FirstName);
        }

        var connections = await query.AsNoTracking()
            .Skip(request.Skip)
            .Take(request.Take)
            .Include(x => x.To)
            .ToListAsync(ctx);

        return Result.Success(
            new UserConnectionResponse.GetFriends
            {
                Connections = connections.Select(UserConnectionMapper.ToGetDto),
                TotalCount = totalCount
            }
        );
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>>
        GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext
            .Users
            .SingleOrDefaultAsync(x => x.AccountId == sessionContextProvider.User!.GetUserId(), ctx);

        if (loggedInUser is null)
        {
            return Result.Unauthorized("You are not authorized to fetch user connections.");
        }

        var existingConnectionIds = dbContext
                .Users
                .Where(u => u.AccountId == userId)
                .SelectMany(u => u.Connections)
                .Where(c =>
                    c.ConnectionType.Equals(UserConnectionType.Friend)
                    || c.ConnectionType.Equals(UserConnectionType.RequestIncoming)
                    || c.ConnectionType.Equals(UserConnectionType.RequestOutgoing))
                .Select(c => c.To.AccountId)
                .ToList();

        var query = dbContext
            .Users
            .Where(u => u.AccountId != userId && !existingConnectionIds.Contains(u.AccountId));

        var totalCount = await query.CountAsync(ctx);

        var suggestedFriends = await query
            .Skip(req.Skip)
            .Take(req.Take)
            .ToListAsync(ctx);

        return Result.Success(
            new UserConnectionResponse.GetSuggestions
            {
                Users = suggestedFriends.Select(UserConnectionMapper.ToGetDto),
                TotalCount = totalCount
            }
        );
    }

    public async Task<Result<UserConnectionResponse.SendFriendRequest>>
        SendFriendRequestAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .Include(u => u.Connections)
                .ThenInclude(c => c.From)
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .Include(u => u.Connections)
                .ThenInclude(c => c.From)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.SendFriendRequest(targetUser);

        if (!result.IsSuccess)
            return Result.Conflict(string.Join(',', result.Errors));

        await dbContext.SaveChangesAsync(ctx);

        return Result.Success(
            new UserConnectionResponse.SendFriendRequest()
            {
                Message = result.SuccessMessage
            }
        );
    }

    public async Task<Result<UserConnectionResponse.AcceptFriendRequest>>
        AcceptFriendRequestAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .Include(u => u.Connections)
                .ThenInclude(c => c.From)
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .Include(u => u.Connections)
                .ThenInclude(c => c.From)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.AcceptFriendRequest(targetUser);

        if (!result.IsSuccess)
            return Result.Conflict(string.Join(',', result.Errors));

        await dbContext.SaveChangesAsync(ctx);

        return Result.Success(
            new UserConnectionResponse.AcceptFriendRequest()
            {
                Message = result.SuccessMessage
            }
        );
    }

    public async Task<Result<UserConnectionResponse.RejectFriendRequest>>
        RejectFriendRequestAsync(string targetAccountId, CancellationToken ctx = default)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .Include(u => u.Connections)
                .ThenInclude(c => c.From)
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .Include(u => u.Connections)
                .ThenInclude(c => c.From)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.RemoveFriendRequest(targetUser);

        return Result.Success(
            new UserConnectionResponse.RejectFriendRequest()
            {
                Message = result.SuccessMessage
            }
        );
    }
}