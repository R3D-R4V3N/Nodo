namespace Rise.Shared.Notifications;

/// <summary>
/// Shape of the browser push subscription sent from the client.
/// </summary>
public record NotificationSubscriptionDto
{
    public required string Endpoint { get; init; }
    public required NotificationSubscriptionKeysDto Keys { get; init; }
}

public record NotificationSubscriptionKeysDto
{
    public required string P256dh { get; init; }
    public required string Auth { get; init; }
}
