using System.Text.Json;
using Microsoft.Extensions.Options;
using WebPush;
using Rise.Services.Notifications;

namespace Rise.Server.Push;

public class PushNotificationService : IPushNotificationService
{
    private readonly VapidOptions _vapidOptions;
    private readonly IPushSubscriptionStore _subscriptionStore;

    public PushNotificationService(
        IOptions<VapidOptions> vapidOptions,
        IPushSubscriptionStore subscriptionStore)
    {
        _vapidOptions = vapidOptions.Value;
        _subscriptionStore = subscriptionStore;
    }

    public async Task SendMessageNotificationAsync(
        string recipientAccountId,
        string senderDisplayName,
        string messagePreview,
        string url,
        CancellationToken cancellationToken = default)
    {
        var subscriptions = await _subscriptionStore.GetByUserIdAsync(recipientAccountId, cancellationToken);

        if (!subscriptions.Any())
            return;

        var payload = JsonSerializer.Serialize(new
        {
            title = $"{senderDisplayName}",
            body = messagePreview,
            url
        });

        var vapidDetails = new VapidDetails(
            _vapidOptions.Subject,
            _vapidOptions.PublicKey,
            _vapidOptions.PrivateKey);

        var client = new WebPushClient();

        foreach (var sub in subscriptions)
        {
            var pushSub = new WebPush.PushSubscription(sub.Endpoint, sub.P256dh, sub.Auth);

            try
            {
                await client.SendNotificationAsync(pushSub, payload, vapidDetails, cancellationToken);
            }
            catch (WebPushException ex)
            {
                // 404 / 410 = subscription niet meer geldig → hier zou je hem later uit de store kunnen verwijderen.
                if (ex.StatusCode is System.Net.HttpStatusCode.Gone or System.Net.HttpStatusCode.NotFound)
                {
                    // TODO: subscription uit store halen
                }

                // Voor nu: fout negeren zodat chatflow nooit stuk gaat door push.
                Console.WriteLine($"Push error naar {recipientAccountId}: {ex.Message}");
            }
        }
    }
}
