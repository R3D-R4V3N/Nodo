using Microsoft.AspNetCore.Components;

namespace Rise.Client.Helpdesk.Components;

public partial class HelpdeskButton
{
    [Parameter] public string Title { get; set; }

    [Parameter] public string BackgroundColor { get; set; }

    [Parameter] public string IconSvg { get; set; }

    [Parameter] public EventCallback OnClick { get; set; }

    [Parameter] public string TitleCss { get; set; } = "";
}