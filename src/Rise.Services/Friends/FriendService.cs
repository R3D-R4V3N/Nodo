using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Friends;
using Rise.Shared.Identity;

namespace Rise.Services.Friends;

public class FriendService(ApplicationDbContext dbContext, ISessionContextProvider sessionContextProvider) : IFriendService
{
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
}
