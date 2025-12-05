using Ardalis.Result;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Notifications;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Shared.Notifications;

namespace Rise.Services.Notifications;

public class NotificationSubscriptionService(ApplicationDbContext dbContext) : INotificationSubscriptionService
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Result> SaveSubscriptionAsync(string accountId, NotificationSubscriptionDto subscription, CancellationToken cancellationToken = default)
    {
        var user = await FindUserByAccountIdAsync(accountId, cancellationToken);
        if (user is null)
        {
            return Result.Unauthorized("Gebruiker kon niet gevonden worden voor push notificaties.");
        }

        var existingSubscriptions = await _dbContext.NotificationSubscriptions
            .Where(s => s.UserId == user.Id && s.Endpoint == subscription.Endpoint)
            .ToListAsync(cancellationToken);

        NotificationSubscription? current = existingSubscriptions.FirstOrDefault();
        if (current is null)
        {
            current = new NotificationSubscription
            {
                UserId = user.Id,
                CreatedAt = DateTime.UtcNow
            };

            _dbContext.NotificationSubscriptions.Add(current);
        }

        current.Endpoint = subscription.Endpoint;
        current.P256dh = subscription.Keys.P256dh;
        current.Auth = subscription.Keys.Auth;
        current.UpdatedAt = DateTime.UtcNow;

        // Remove duplicates if they exist.
        if (existingSubscriptions.Count > 1)
        {
            _dbContext.NotificationSubscriptions.RemoveRange(existingSubscriptions.Where(s => s != current));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    public async Task<IReadOnlyList<NotificationSubscription>> GetSubscriptionsForUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default)
    {
        var ids = userIds.Distinct().ToArray();
        if (ids.Length == 0)
        {
            return Array.Empty<NotificationSubscription>();
        }

        return await _dbContext.NotificationSubscriptions
            .Where(s => ids.Contains(s.UserId))
            .ToListAsync(cancellationToken);
    }

    private async Task<BaseUser?> FindUserByAccountIdAsync(string accountId, CancellationToken cancellationToken)
    {
        return await _dbContext.Set<BaseUser>()
            .SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken)
            ?? await _dbContext.Supervisors.SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken)
            ?? await _dbContext.Users.SingleOrDefaultAsync(u => u.AccountId == accountId, cancellationToken);
    }
}
