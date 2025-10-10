using System;

namespace Rise.Shared.Chats;

public sealed class SupervisorAlertNotificationDto
{
    public int ChatId { get; init; }
    public bool IsActive { get; init; }
    public string TriggeredByName { get; init; } = string.Empty;
    public DateTimeOffset Timestamp { get; init; }
}
