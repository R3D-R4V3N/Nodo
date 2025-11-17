using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Hobbys;
using Rise.Domain.Users.Sentiment;
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
public class UserConnectionService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider,
    IUserConnectionNotificationDispatcher? notificationDispatcher = null) : IUserConnectionService
{
    private readonly IUserConnectionNotificationDispatcher? _notificationDispatcher = notificationDispatcher;
    public async Task<Result<UserConnectionResponse.GetFriends>>
        GetFriendsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.Users
            .SingleOrDefaultAsync(x => x.AccountId == userId, ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("You are not authorized to fetch user connections.");

        var connectionsQuery = dbContext
            .UserConnections
            .Include(c => c.To)
            .Where(c => c.From.AccountId == userId 
                && c.ConnectionType == UserConnectionType.Friend);

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            connectionsQuery = connectionsQuery.Where(c =>
                c.To.FirstName.Value.Contains(request.SearchTerm) ||
                c.To.LastName.Value.Contains(request.SearchTerm));
        }

        var totalCount = await connectionsQuery.CountAsync(ctx);

        var connections =  connectionsQuery
            .AsEnumerable() 
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.To.FirstName.Value)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return Result.Success(new UserConnectionResponse.GetFriends
        {
            Connections = connections.Select(UserConnectionMapper.ToGetDto),
            TotalCount = totalCount
        });
    }

    public async Task<Result<UserConnectionResponse.GetFriendRequests>>
        GetFriendRequestsAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.Users
            .SingleOrDefaultAsync(x => x.AccountId == userId, ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("You are not authorized to fetch user connections.");

        var connectionsQuery = dbContext
            .UserConnections
            .Include(c => c.To)
            .Where(c => c.From.AccountId == userId 
                && (c.ConnectionType == UserConnectionType.RequestIncoming 
                    || c.ConnectionType == UserConnectionType.RequestOutgoing));

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            connectionsQuery = connectionsQuery.Where(c =>
                c.To.FirstName.Value.Contains(request.SearchTerm) ||
                c.To.LastName.Value.Contains(request.SearchTerm));
        }

        var totalCount = await connectionsQuery.CountAsync(ctx);

        var connections = connectionsQuery
            .AsEnumerable()
            .OrderByDescending(c => c.CreatedAt)
            .ThenBy(c => c.To.FirstName.Value)
            .Skip(request.Skip)
            .Take(request.Take)
            .ToList();

        return Result.Success(new UserConnectionResponse.GetFriendRequests
        {
            Connections = connections.Select(UserConnectionMapper.ToGetDto),
            TotalCount = totalCount
        });
    }

    public async Task<Result<UserConnectionResponse.GetSuggestions>>
        GetSuggestedFriendsAsync(QueryRequest.SkipTake req, CancellationToken ctx = default)
    {
        var userId = sessionContextProvider.User!.GetUserId();

        var loggedInUser = await dbContext.Users
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .SingleOrDefaultAsync(x => x.AccountId == userId, ctx);

        if (loggedInUser is null)
            return Result.Unauthorized("You are not authorized to fetch user connections.");

        var existingConnectionIds = await dbContext
            .Users
            .AsNoTracking()
            .Where(u => u.AccountId == userId)
            .SelectMany(u => u.Connections)
            .Where(c => c.ConnectionType == UserConnectionType.Friend
                     || c.ConnectionType == UserConnectionType.RequestIncoming
                     || c.ConnectionType == UserConnectionType.RequestOutgoing
                     || c.ConnectionType == UserConnectionType.Blocked)
            .Select(c => c.To.AccountId)
            .ToListAsync(ctx);

        var likedCategories = loggedInUser
            .Likes
            .Select(s => s.Category)
            .Distinct()
            .ToList();

        var hobbyTypes = loggedInUser
            .Hobbies
            .Select(h => h.Hobby)
            .Distinct()
            .ToList();

        var candidatesQuery = dbContext
            .Users
            .AsNoTracking()
            .Include(u => u.Sentiments)
            .Include(u => u.Hobbies)
            .Where(u => u.AccountId != userId && !existingConnectionIds.Contains(u.AccountId));

        var totalCount = await candidatesQuery.CountAsync(ctx);

        var rankedSuggestions = await candidatesQuery
            .Select(u => new
            {
                User = u,
                SharedLikesCount = likedCategories
                    .Intersect(u.Sentiments
                        .Where(s => s.Type == SentimentType.Like)
                        .Select(s => s.Category)
                    ).Count(),
                SharedHobbiesCount = hobbyTypes
                    .Intersect(u.Hobbies.Select(h => h.Hobby))
                    .Count()
            })
            .OrderByDescending(x => x.SharedLikesCount + x.SharedHobbiesCount)
            .ThenBy(x => x.User.FirstName.Value)
            .ThenBy(x => x.User.LastName.Value)
            .Select(x => x.User)
            .Skip(req.Skip)
            .Take(req.Take)
            .ToListAsync(ctx);

        return Result.Success(new UserConnectionResponse.GetSuggestions
        {
            Users = rankedSuggestions.Select(UserConnectionMapper.ToGetDto),
            TotalCount = totalCount
        });
    }

    public async Task<Result<UserConnectionResponse.CancelFriendRequest>> 
        CancelFriendRequest(string targetAccountId, CancellationToken ctx = default)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.CancelFriendRequest(targetUser);

        if (!result.IsSuccess)
            return Result.Conflict(string.Join(',', result.Errors));

        await dbContext.SaveChangesAsync(ctx);

        await NotifyBothSidesAsync(currentUserId, targetAccountId, ctx);

        return Result.Success(
            new UserConnectionResponse.CancelFriendRequest()
            {
                Message = result.SuccessMessage
            }
        );
    }

    public async Task<Result<UserConnectionResponse.RemoveFriendRequest>> 
        RemoveFriendAsync(string targetAccountId, CancellationToken ctx)
    {
        var currentUserId = sessionContextProvider.User!.GetUserId();

        // Huidige gebruiker ophalen
        var currentUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId , ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.RemoveFriend(targetUser);

        if (!result.IsSuccess)
            return Result.Conflict(string.Join(',', result.Errors));

        await dbContext.SaveChangesAsync(ctx);

        await NotifyBothSidesAsync(currentUserId, targetAccountId, ctx);

        return Result.Success(
            new UserConnectionResponse.RemoveFriendRequest
            {
                Message = result.SuccessMessage
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
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.SendFriendRequest(targetUser);

        if (!result.IsSuccess)
            return Result.Conflict(string.Join(',', result.Errors));

        await dbContext.SaveChangesAsync(ctx);

        await NotifyBothSidesAsync(currentUserId, targetAccountId, ctx);

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
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.AcceptFriendRequest(targetUser);

        if (!result.IsSuccess)
            return Result.Conflict(string.Join(',', result.Errors));

        await dbContext.SaveChangesAsync(ctx);

        await NotifyBothSidesAsync(currentUserId, targetAccountId, ctx);

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
            .SingleOrDefaultAsync(u => u.AccountId == currentUserId, ctx);

        // Doelgebruiker ophalen
        var targetUser = await dbContext
            .Users
            .Include(u => u.Connections)
                .ThenInclude(c => c.To)
            .SingleOrDefaultAsync(u => u.AccountId == targetAccountId, ctx);

        if (currentUser is null || targetUser is null)
            return Result.NotFound("Gebruiker niet gevonden.");

        var result = currentUser.RejectFriendRequest(targetUser);

        if (!result.IsSuccess)
            return Result.Conflict(string.Join(',', result.Errors));

        await dbContext.SaveChangesAsync(ctx);

        await NotifyBothSidesAsync(currentUserId, targetAccountId, ctx);

        return Result.Success(
            new UserConnectionResponse.RejectFriendRequest()
            {
                Message = result.SuccessMessage
            }
        );
    }
    private async Task NotifyConnectionsChangedAsync(string accountId, CancellationToken ctx)
    {
        if (_notificationDispatcher is null || string.IsNullOrWhiteSpace(accountId))
        {
            return;
        }

        try
        {
            await _notificationDispatcher.NotifyFriendConnectionsChangedAsync(accountId, ctx);
        }
        catch
        {
            // Realtime updates are best-effort only.
        }
    }

    private Task NotifyBothSidesAsync(string currentAccountId, string targetAccountId, CancellationToken ctx)
    {
        return Task.WhenAll(
            NotifyConnectionsChangedAsync(currentAccountId, ctx),
            NotifyConnectionsChangedAsync(targetAccountId, ctx)
        );
    }
}
