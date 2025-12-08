namespace Rise.Services.Notifications;

public interface IPushNotificationService
{
    /// <summary>
    /// Stuurt een pushnotificatie naar een gebruiker met een simpel message-patroon.
    /// </summary>
    Task SendMessageNotificationAsync(
        string recipientUserId,
        string senderName,
        string messagePreview,
        string url,
        CancellationToken cancellationToken = default);
}