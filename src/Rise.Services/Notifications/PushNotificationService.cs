using System.Linq;
using System.Net;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rise.Persistence;
using Rise.Shared.Chats;
using WebPush;

namespace Rise.Services.Notifications;

public class PushNotificationService(
    ApplicationDbContext dbContext,
    INotificationSubscriptionService subscriptionService,
    IOptions<VapidSettings> vapidOptions,
    ILogger<PushNotificationService> logger) : IPushNotificationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly INotificationSubscriptionService _subscriptionService = subscriptionService;
    private readonly VapidSettings _vapidSettings = vapidOptions.Value;
    private readonly ILogger<PushNotificationService> _logger = logger;

    public async Task SendChatMessageNotificationAsync(int chatId, MessageDto.Chat message, CancellationToken cancellationToken = default)
    {
        var chat = await _dbContext.Chats
            .Include(c => c.Users)
            .SingleOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        if (chat is null)
        {
            return;
        }

        var senderAccountId = message.User?.AccountId;
        var recipientIds = chat.Users
            .Where(u => senderAccountId is null || !u.AccountId.Equals(senderAccountId, StringComparison.OrdinalIgnoreCase))
            .Select(u => u.Id)
            .ToArray();

        var subscriptions = await _subscriptionService.GetSubscriptionsForUsersAsync(recipientIds, cancellationToken);
        if (subscriptions.Count == 0)
        {
            return;
        }

        var payload = JsonSerializer.Serialize(new
        {
            title = "Nieuw chatbericht",
            body = $"{message.User?.Name ?? "Nodo"}: {BuildMessageSummary(message)}",
            data = new
            {
                chatId,
                messageId = message.Id,
                sender = message.User?.Name,
            }
        });

        var vapidDetails = new VapidDetails(_vapidSettings.Subject, _vapidSettings.PublicKey, _vapidSettings.PrivateKey);
        var webPushClient = new WebPushClient();

        foreach (var subscription in subscriptions)
        {
            try
            {
                var pushSubscription = new PushSubscription(subscription.Endpoint, subscription.P256dh, subscription.Auth);
                await webPushClient.SendNotificationAsync(pushSubscription, payload, vapidDetails, cancellationToken: cancellationToken);
            }
            catch (WebPushException ex) when (ex.StatusCode is HttpStatusCode.Gone or HttpStatusCode.NotFound)
            {
                // Subscription expired or removed on the browser: delete it.
                _logger.LogWarning(ex, "Push subscription expired for endpoint {Endpoint}", subscription.Endpoint);
                _dbContext.NotificationSubscriptions.Remove(subscription);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Unable to send push notification to {Endpoint}", subscription.Endpoint);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    private static string BuildMessageSummary(MessageDto.Chat message)
    {
        if (!string.IsNullOrWhiteSpace(message.Content))
        {
            return message.Content.Length > 64 ? message.Content[..64] + "…" : message.Content;
        }

        if (!string.IsNullOrWhiteSpace(message.AudioDataBlob))
        {
            return "Nieuw spraakbericht";
        }

        return "Nieuw bericht";
    }
}
