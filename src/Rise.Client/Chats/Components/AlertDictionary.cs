using Microsoft.AspNetCore.Components;
using System.Collections.Generic;

namespace Rise.Client.Chats.Components;

public static class AlertDictionary
{
    public static IReadOnlyList<AlertPrompt.AlertReason> Reasons { get; } = new List<AlertPrompt.AlertReason>
    {
        new("Ongepast gedrag", MapPinIcon),
        new("Spam of fraude", SnowflakeIcon),
        new("Ander probleem", BandageIcon)
    };

    private static RenderFragment MapPinIcon => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddMultipleAttributes(1, new Dictionary<string, object?>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "currentColor",
            ["aria-hidden"] = "true",
            ["class"] = "h-6 w-6"
        });
        builder.OpenElement(2, "path");
        builder.AddAttribute(3, "d", "M12 21.75a.75.75 0 0 1-.624-.334C9.273 18.45 6.75 14.047 6.75 10.5A5.25 5.25 0 0 1 12 5.25a5.25 5.25 0 0 1 5.25 5.25c0 3.547-2.523 7.95-4.626 10.916a.75.75 0 0 1-.624.334ZM12 12.75a2.25 2.25 0 1 0 0-4.5 2.25 2.25 0 0 0 0 4.5Z");
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment SnowflakeIcon => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddMultipleAttributes(1, new Dictionary<string, object?>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "currentColor",
            ["aria-hidden"] = "true",
            ["class"] = "h-6 w-6"
        });
        builder.OpenElement(2, "path");
        builder.AddAttribute(3, "d", "M11.25 2.25a.75.75 0 0 1 1.5 0V5l1.436-1.436a.75.75 0 1 1 1.061 1.061L12.75 6.122v1.43l2.059-1.187 1.15-3.083a.75.75 0 1 1 1.414.527l-.64 1.72 1.72-.64a.75.75 0 1 1 .527 1.414l-3.083 1.15-2.147 1.238 2.147 1.237 3.083 1.15a.75.75 0 1 1-.527 1.414l-1.72-.64.64 1.72a.75.75 0 0 1-1.414.527l-1.15-3.083-2.059-1.187v1.43l2.497 1.497a.75.75 0 1 1-.776 1.286L12.75 14.5v2.378l1.997 2.29a.75.75 0 0 1-1.14.976L12 17.94l-1.607 1.935a.75.75 0 0 1-1.14-.976l1.997-2.29V14.5l-1.721 1.035a.75.75 0 1 1-.776-1.286l2.497-1.497v-1.43l-2.059 1.187-1.15 3.083a.75.75 0 1 1-1.414-.527l.64-1.72-1.72.64a.75.75 0 0 1-.527-1.414l3.083-1.15 2.147-1.237-2.147-1.238-3.083-1.15a.75.75 0 1 1 .527-1.414l1.72.64-.64-1.72a.75.75 0 0 1 1.414-.527l1.15 3.083 2.059 1.187v-1.43l-2.247-1.32a.75.75 0 1 1 .776-1.286l1.471.864Z");
        builder.CloseElement();
        builder.CloseElement();
    };

    private static RenderFragment BandageIcon => builder =>
    {
        builder.OpenElement(0, "svg");
        builder.AddMultipleAttributes(1, new Dictionary<string, object?>
        {
            ["xmlns"] = "http://www.w3.org/2000/svg",
            ["viewBox"] = "0 0 24 24",
            ["fill"] = "currentColor",
            ["aria-hidden"] = "true",
            ["class"] = "h-6 w-6"
        });
        builder.OpenElement(2, "path");
        builder.AddAttribute(3, "d", "M8.28 3.22a3.75 3.75 0 0 1 5.303 0l3.197 3.197a3.75 3.75 0 0 1 0 5.303l-4.8 4.8a3.75 3.75 0 0 1-5.303 0l-3.197-3.197a3.75 3.75 0 0 1 0-5.303l4.8-4.8Zm1.133 1.133-4.8 4.8a2.25 2.25 0 0 0 0 3.182l3.197 3.197a2.25 2.25 0 0 0 3.182 0l4.8-4.8a2.25 2.25 0 0 0 0-3.182l-3.197-3.197a2.25 2.25 0 0 0-3.182 0Zm1.607 1.31a.75.75 0 0 1 1.06 0l3.9 3.9a.75.75 0 0 1-1.06 1.06l-3.9-3.9a.75.75 0 0 1 0-1.06Zm-1.43 4.37a.75.75 0 1 1 1.06-1.06.75.75 0 0 1-1.06 1.06Zm2.5-2.5a.75.75 0 1 1 1.06-1.06.75.75 0 0 1-1.06 1.06Zm-2.5 2.5a.75.75 0 1 1 1.06-1.06.75.75 0 0 1-1.06 1.06Zm-2.5 2.5a.75.75 0 0 1 1.06-1.06.75.75 0 0 1-1.06 1.06Z");
        builder.CloseElement();
        builder.CloseElement();
    };
}
