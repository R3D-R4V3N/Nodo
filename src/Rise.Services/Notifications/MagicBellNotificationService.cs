using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Rise.Domain.Messages;
using Rise.Domain.Users;

namespace Rise.Services.Notifications;

public class MagicBellNotificationService(
    HttpClient httpClient,
    IOptions<MagicBellOptions> options,
    UserManager<IdentityUser> userManager,
    ILogger<MagicBellNotificationService> logger) : IMagicBellNotificationService
{
    private readonly HttpClient _httpClient = httpClient;
    private readonly MagicBellOptions _options = options.Value;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly ILogger<MagicBellNotificationService> _logger = logger;

    public async Task NotifyChatMessageAsync(Message message, IEnumerable<BaseUser> recipients, CancellationToken cancellationToken = default)
    {
        if (!IsConfigured())
        {
            _logger.LogDebug("MagicBell is not configured; skipping push notification dispatch.");
            return;
        }

        var recipientsList = recipients?.ToList();
        if (recipientsList is null || recipientsList.Count == 0)
        {
            return;
        }

        var accountIds = recipientsList
            .Select(user => user.AccountId)
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Distinct(StringComparer.Ordinal)
            .ToList();

        if (accountIds.Count == 0)
        {
            return;
        }

        var identityUsers = await _userManager.Users
            .Where(u => accountIds.Contains(u.Id))
            .ToListAsync(cancellationToken);

        var payloadRecipients = identityUsers
            .Select(u => new { external_id = u.Id, email = u.Email })
            .Where(r => !string.IsNullOrWhiteSpace(r.email))
            .ToList();

        if (payloadRecipients.Count == 0)
        {
            return;
        }

        var preview = BuildContentPreview(message);

        var payload = new
        {
            notification = new
            {
                title = $"Nieuw bericht van {message.Sender}",
                content = preview,
                category = "chat-message",
                custom_attributes = new
                {
                    chatId = message.Chat?.Id ?? 0,
                    senderId = message.Sender.Id,
                    senderAccountId = message.Sender.AccountId,
                    hasAudio = message.AudioData is not null
                },
                recipients = payloadRecipients
            }
        };

        try
        {
            var response = await _httpClient.PostAsJsonAsync("notifications", payload, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("MagicBell push failed with status {StatusCode}: {Error}", response.StatusCode, error);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sending MagicBell push notification failed.");
        }
    }

    private bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(_options.ApiKey)
            && !string.IsNullOrWhiteSpace(_options.ApiSecret)
            && _httpClient.DefaultRequestHeaders.Contains("X-MAGICBELL-API-KEY")
            && _httpClient.DefaultRequestHeaders.Contains("X-MAGICBELL-API-SECRET");
    }

    private static string BuildContentPreview(Message message)
    {
        if (message.Text is not null && !string.IsNullOrWhiteSpace(message.Text.CleanedUpValue))
        {
            var text = message.Text.CleanedUpValue;
            return text.Length > 160 ? text[..160] + "…" : text;
        }

        if (message.AudioData is not null)
        {
            return "Je hebt een audiobericht ontvangen.";
        }

        return "Je hebt een nieuw bericht ontvangen.";
    }
}
