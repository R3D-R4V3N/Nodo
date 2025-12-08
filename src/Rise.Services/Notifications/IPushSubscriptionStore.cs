using System.Collections.Generic;

namespace Rise.Services.Notifications;

public interface IPushSubscriptionStore
{
    Task SaveAsync(PushSubscriptionModel subscription, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<PushSubscriptionModel>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
}