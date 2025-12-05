using System.Security.Claims;
using Rise.Services.Notifications;
using Rise.Shared.Notifications;

namespace Rise.Server.Endpoints.Notifications;

public class Subscribe(INotificationSubscriptionService subscriptionService)
    : Endpoint<NotificationSubscriptionDto, Result>
{
    public override void Configure()
    {
        Post("/api/notifications/subscribe");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override async Task<Result> ExecuteAsync(NotificationSubscriptionDto req, CancellationToken ct)
    {
        var accountId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        return await subscriptionService.SaveSubscriptionAsync(accountId, req, ct);
    }
}
