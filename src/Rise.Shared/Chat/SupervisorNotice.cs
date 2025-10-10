using System;

namespace Rise.Shared.Chat;

public sealed record SupervisorNotice(
    string Message,
    DateTimeOffset Timestamp,
    string TriggeredByName
);
