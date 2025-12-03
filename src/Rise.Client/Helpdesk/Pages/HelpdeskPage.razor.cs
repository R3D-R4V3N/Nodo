namespace Rise.Client.Helpdesk.Pages;

public partial class HelpdeskPage
{
    private void NavigateToHulpRobot()
    {
        NavigationManager.NavigateTo($"/helpdesk/hulp-robot");
    }

    private void NavigateToVideos()
    {
        NavigationManager.NavigateTo($"/helpdesk/videos");
    }

    private void NavigateToVoorlichting()
    {
        NavigationManager.NavigateTo($"/helpdesk/voorlichting");
    }
}