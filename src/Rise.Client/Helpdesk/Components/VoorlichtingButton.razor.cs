using Microsoft.AspNetCore.Components;

namespace Rise.Client.Helpdesk.Components;

public partial class VoorlichtingButton
{
    [Parameter] public string Link { get; set; }
    [Parameter] public string ImageUrl { get; set; }
    [Parameter] public string ImageAlt { get; set; }
    [Parameter] public string Title { get; set; }
    [Parameter] public string ImageContainerClasses { get; set; }
    [Parameter] public string TitleClasses { get; set; } = "p-4"; // default padding
}