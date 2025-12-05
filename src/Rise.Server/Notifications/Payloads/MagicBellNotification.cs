using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Rise.Server.Notifications.Payloads;

public class MagicBellNotificationRequest
{
    [JsonPropertyName("notification")]
    public MagicBellNotification Notification { get; set; } = new();
}

public class MagicBellNotification
{
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("action_url")]
    public string? ActionUrl { get; set; }

    [JsonPropertyName("recipients")]
    public List<MagicBellRecipient> Recipients { get; set; } = [];
}

public class MagicBellRecipient
{
    [JsonPropertyName("external_id")]
    public string? ExternalId { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }
}
