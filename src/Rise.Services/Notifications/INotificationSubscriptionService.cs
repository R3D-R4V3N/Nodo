using Ardalis.Result;
using Rise.Shared.Notifications;

namespace Rise.Services.Notifications;

public interface INotificationSubscriptionService
{
    Task<Result> SaveSubscriptionAsync(string accountId, NotificationSubscriptionDto subscription, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Domain.Notifications.NotificationSubscription>> GetSubscriptionsForUsersAsync(IEnumerable<int> userIds, CancellationToken cancellationToken = default);
}
