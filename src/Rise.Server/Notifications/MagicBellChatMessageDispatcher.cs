using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rise.Persistence;
using Rise.Server.Notifications.Payloads;
using Rise.Services.Chats;
using Rise.Shared.Chats;

namespace Rise.Server.Notifications;

public class MagicBellChatMessageDispatcher(
    IHttpClientFactory httpClientFactory,
    IOptions<MagicBellOptions> options,
    ApplicationDbContext dbContext,
    ILogger<MagicBellChatMessageDispatcher> logger) : IChatMessageDispatcher
{
    private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
    private readonly MagicBellOptions _options = options.Value;
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ILogger<MagicBellChatMessageDispatcher> _logger = logger;

    public async Task NotifyMessageCreatedAsync(int chatId, MessageDto.Chat message, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
        {
            return;
        }

        var chat = await _dbContext.Chats
            .Include(c => c.Users)
            .SingleOrDefaultAsync(c => c.Id == chatId, cancellationToken);

        if (chat is null)
        {
            return;
        }

        var recipients = chat.Users
            .Where(user => !string.IsNullOrWhiteSpace(user.AccountId))
            .Where(user => !string.Equals(user.AccountId, message.User.AccountId, StringComparison.OrdinalIgnoreCase))
            .Select(user => new MagicBellRecipient { ExternalId = user.AccountId })
            .ToList();

        if (recipients.Count == 0)
        {
            return;
        }

        var notification = new MagicBellNotificationRequest
        {
            Notification = new MagicBellNotification
            {
                Title = $"Nieuw bericht van {message.User.Name}",
                Content = BuildContent(message),
                ActionUrl = BuildActionUrl(chatId),
                Recipients = recipients
            }
        };

        await SendAsync(notification, cancellationToken);
    }

    private async Task SendAsync(MagicBellNotificationRequest request, CancellationToken cancellationToken)
    {
        var client = _httpClientFactory.CreateClient(MagicBellOptions.HttpClientName);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "notifications")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.TryAddWithoutValidation("X-MAGICBELL-API-KEY", _options.ApiKey);
        httpRequest.Headers.TryAddWithoutValidation("X-MAGICBELL-API-SECRET", _options.ApiSecret);

        try
        {
            var response = await client.SendAsync(httpRequest, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning(
                    "MagicBell notification failed with status {StatusCode}: {Response}",
                    response.StatusCode,
                    body);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "MagicBell notification could not be sent");
        }
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.ApiKey)
            && !string.IsNullOrWhiteSpace(_options.ApiSecret);
    }

    private static string BuildContent(MessageDto.Chat message)
    {
        if (!string.IsNullOrWhiteSpace(message.Content))
        {
            return message.Content.Length > 160
                ? message.Content[..160] + "…"
                : message.Content;
        }

        if (!string.IsNullOrWhiteSpace(message.AudioDataBlob))
        {
            return "Je hebt een spraakbericht ontvangen.";
        }

        return "Je hebt een nieuw bericht ontvangen.";
    }

    private string? BuildActionUrl(int chatId)
    {
        if (string.IsNullOrWhiteSpace(_options.ActionUrlTemplate))
        {
            return null;
        }

        return _options.ActionUrlTemplate.Replace("{chatId}", chatId.ToString(), StringComparison.OrdinalIgnoreCase);
    }
}
