using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Notifications;

/// <summary>
/// Represents a web push subscription for a specific user/device.
/// </summary>
public class NotificationSubscription : Entity
{
    public int UserId { get; set; }
    public BaseUser User { get; set; } = default!;

    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string Auth { get; set; } = string.Empty;
}
