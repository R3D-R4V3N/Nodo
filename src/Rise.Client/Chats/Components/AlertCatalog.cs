using Rise.Client.Chats.Components;

public static class AlertCatalog
{
    public static IReadOnlyList<AlertPrompt.AlertReason> Reasons { get; } =
        new List<AlertPrompt.AlertReason>
        {
            new("Pesten", "/images/discrimination.png"),
            new("Uitsluiten", "/images/bully.png"),
            new("Ongepast taalgebruik", "/images/cyber-bullying.png"),
            new("Ander probleem", "/images/menu.png")
        };
}