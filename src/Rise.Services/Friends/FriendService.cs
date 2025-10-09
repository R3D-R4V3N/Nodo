using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Friends;
using Rise.Shared.Identity;

namespace Rise.Services.Friends;

public class FriendService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IFriendService
{
    public async Task<Result<FriendResponse.Index>> GetFriendsAsync(CancellationToken ctx = default)
    {
        var accountId = sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
            return Result.Unauthorized();

        var currentUser = await dbContext.ApplicationUsers
            .Include(u => u.Friends)
            .Include(u => u.FriendRequests)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (currentUser is null)
            return Result.NotFound($"User with account '{accountId}' was not found.");

        var friends = currentUser.Friends
            .Select(friend => MapToDto(friend, FriendState.All))
            .OrderBy(friend => friend.Name)
            .ToList();

        var requests = currentUser.FriendRequests
            .Select(request => MapToDto(request, FriendState.Requests))
            .OrderBy(request => request.Name)
            .ToList();

        var excludedIds = new HashSet<int>(friends.Select(f => f.Id))
        {
            currentUser.Id
        };
        foreach (var request in requests)
            excludedIds.Add(request.Id);

        var suggestions = await dbContext.ApplicationUsers
            .AsNoTracking()
            .Where(user => !excludedIds.Contains(user.Id))
            .OrderBy(user => user.FirstName)
            .ThenBy(user => user.LastName)
            .Select(user => MapToDto(user, FriendState.Add))
            .ToListAsync(ctx);

        return Result.Success(new FriendResponse.Index
        {
            Friends = friends,
            Requests = requests,
            Suggestions = suggestions
        });
    }

    public async Task<Result> AddFriendAsync(FriendRequest.Add request, CancellationToken ctx = default)
    {
        var accountId = sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
            return Result.Unauthorized();

        var currentUser = await dbContext.ApplicationUsers
            .Include(u => u.Friends)
            .Include(u => u.FriendRequests)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (currentUser is null)
            return Result.NotFound($"User with account '{accountId}' was not found.");

        var friend = await dbContext.ApplicationUsers
            .Include(u => u.Friends)
            .Include(u => u.FriendRequests)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.Id == request.FriendId, ctx);

        if (friend is null)
            return Result.NotFound($"Friend with id '{request.FriendId}' was not found.");

        var result = currentUser.AddFriend(friend);
        if (!result.IsSuccess)
            return result;

        await dbContext.SaveChangesAsync(ctx);
        return result;
    }

    public async Task<Result> RemoveFriendAsync(FriendRequest.Remove request, CancellationToken ctx = default)
    {
        var accountId = sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
            return Result.Unauthorized();

        var currentUser = await dbContext.ApplicationUsers
            .Include(u => u.Friends)
            .Include(u => u.FriendRequests)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.AccountId == accountId, ctx);

        if (currentUser is null)
            return Result.NotFound($"User with account '{accountId}' was not found.");

        var friend = await dbContext.ApplicationUsers
            .Include(u => u.Friends)
            .Include(u => u.FriendRequests)
            .AsSplitQuery()
            .SingleOrDefaultAsync(u => u.Id == request.FriendId, ctx);

        if (friend is null)
            return Result.NotFound($"Friend with id '{request.FriendId}' was not found.");

        var result = currentUser.RemoveFriend(friend);
        if (!result.IsSuccess)
            return result;

        await dbContext.SaveChangesAsync(ctx);
        return result;
    }

    private static FriendDto MapToDto(ApplicationUser user, FriendState state)
    {
        var name = string.Join(" ", new[] { user.FirstName, user.LastName }.Where(x => !string.IsNullOrWhiteSpace(x)));
        if (string.IsNullOrWhiteSpace(name))
            name = "Onbekende gebruiker";

        return new FriendDto
        {
            Id = user.Id,
            Name = name,
            Biography = user.Biography,
            AvatarUrl = BuildAvatarUrl(name),
            State = state
        };
    }

    private static string BuildAvatarUrl(string name)
    {
        return $"https://ui-avatars.com/api/?background=127646&color=fff&name={Uri.EscapeDataString(name)}";
    }
}
