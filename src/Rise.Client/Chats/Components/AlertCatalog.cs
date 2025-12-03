using System;
using System.Linq;
using Rise.Client.Chats.Components;
using Rise.Shared.Emergencies;

public static class AlertCatalog
{
    public static IReadOnlyList<AlertPrompt.AlertReason> Reasons { get; } =
        Enum.GetValues<EmergencyTypeDto>()
            .Select(type =>
            {
                var (label, icon) = EmergencyDto.TranslateTypeToDisplay(type);
                return new AlertPrompt.AlertReason(type, label, icon);
            })
            .ToList();
}