namespace Rise.Shared.Notifications;

public class PushSubscriptionDto
{
    public string UserId { get; set; } = default!;
    public string Endpoint { get; set; } = default!;
    public string P256dh { get; set; } = default!;
    public string Auth { get; set; } = default!;
}