using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Rise.Services.Notifications;

public class InMemoryPushSubscriptionStore : IPushSubscriptionStore
{
    private readonly ConcurrentDictionary<string, List<PushSubscriptionModel>> _subscriptions = new();

    public Task SaveAsync(PushSubscriptionModel subscription, CancellationToken cancellationToken = default)
    {
        var list = _subscriptions.GetOrAdd(subscription.UserId, _ => new List<PushSubscriptionModel>());

        var existing = list.FirstOrDefault(x => x.Endpoint == subscription.Endpoint);
        if (existing is null)
        {
            list.Add(subscription);
        }
        else
        {
            existing.P256dh = subscription.P256dh;
            existing.Auth = subscription.Auth;
        }

        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<PushSubscriptionModel>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
    {
        if (_subscriptions.TryGetValue(userId, out var list))
        {
            return Task.FromResult((IReadOnlyList<PushSubscriptionModel>)list.ToList());
        }

        return Task.FromResult((IReadOnlyList<PushSubscriptionModel>)new List<PushSubscriptionModel>());
    }
}