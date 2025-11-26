using Rise.Services.UserConnections;

namespace Rise.Services.Tests.Fakers;

public class FakeUserConnectionNotificationDispatcher : IUserConnectionNotificationDispatcher
{
    public List<string> Notifications { get; } = new();
    public Task NotifyFriendConnectionsChangedAsync(string accountId, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrWhiteSpace(accountId))
        {
            Notifications.Add(accountId);
        }

        return Task.CompletedTask;
    }
}