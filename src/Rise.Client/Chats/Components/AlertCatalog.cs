using System.Collections.Generic;

namespace Rise.Client.Chats.Components;

public static class AlertCatalog
{
    public static IReadOnlyList<AlertPrompt.AlertReason> Reasons { get; } = new List<AlertPrompt.AlertReason>
    {
        new("Pesten", PestenHref),
        new("Uitsluiten", UitsluitenHref),
        new("Ongepast taalgebruik", TaalgebruikHref),
        new("Ander probleem", TaalgebruikHref)
    };

    private const string IconBasePath = "/Components/SVG/";

    private const string PestenHref = IconBasePath + "pesten.svg#icon";

    private const string UitsluitenHref = IconBasePath + "uitsluiten.svg#icon";

    private const string TaalgebruikHref = IconBasePath + "taalgebruik.svg#icon";
}
