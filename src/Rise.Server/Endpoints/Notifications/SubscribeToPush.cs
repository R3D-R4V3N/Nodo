using System.Security.Claims;
using Ardalis.Result;
using FastEndpoints;
using Rise.Services.Notifications;
using Rise.Shared.Notifications;

namespace Rise.Server.Endpoints.Notifications;

public class SubscribeToPush(IPushSubscriptionStore store)
    : Endpoint<PushSubscriptionDto, Result>
{
    public override void Configure()
    {
        Post("/api/push/subscribe");
        Claims(ClaimTypes.NameIdentifier);
    }

    public override async Task<Result> ExecuteAsync(PushSubscriptionDto req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.UserId) ||
            string.IsNullOrWhiteSpace(req.Endpoint) ||
            string.IsNullOrWhiteSpace(req.P256dh) ||
            string.IsNullOrWhiteSpace(req.Auth))
        {
            return Result.Error("Missing subscription data.");
        }

        var model = new PushSubscriptionModel
        {
            UserId = req.UserId,
            Endpoint = req.Endpoint,
            P256dh = req.P256dh,
            Auth = req.Auth
        };

        await store.SaveAsync(model, ct);

        return Result.Success();
    }
}