using Rise.Shared.Emergencies;

public static class AlertCatalog
{
    public static IReadOnlyList<AlertPrompt.AlertReason> Reasons { get; } =
        Enum.GetValues<EmergencyTypeDto>()
            .Select(type =>
            {
                var (label, iconHref) = EmergencyDto.TranslateEnumToText(type);
                return new AlertPrompt.AlertReason(type, label, iconHref);
            })
            .ToList();
}