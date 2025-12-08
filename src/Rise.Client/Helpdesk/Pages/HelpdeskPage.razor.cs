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
    private void NavigateToSupervisorChat()
    {
        NavigationManager.NavigateTo($"/helpdesk/SupervisorChat");
    }
    
    private string RobotIcon = @"<svg xmlns='http://www.w3.org/2000/svg' width='67' height='67' fill='#ffffff' viewBox='0 0 256 256'><path d='M200,48H136V16a8,8,0,0,0-16,0V48H56A32,32,0,0,0,24,80V192a32,32,0,0,0,32,32H200a32,32,0,0,0,32-32V80A32,32,0,0,0,200,48ZM172,96a12,12,0,1,1-12,12A12,12,0,0,1,172,96ZM96,184H80a16,16,0,0,1,0-32H96ZM84,120a12,12,0,1,1,12-12A12,12,0,0,1,84,120Zm60,64H112V152h32Zm32,0H160V152h16a16,16,0,0,1,0,32Z'></path></svg>";
    
    private string VideoIcon = @"<svg xmlns='http://www.w3.org/2000/svg' width='67' height='67' fill='#ffffff' viewBox='0 0 256 256'><path d='M232,208a8,8,0,0,1-8,8H32a8,8,0,0,1,0-16H224A8,8,0,0,1,232,208Zm0-152V168a16,16,0,0,1-16,16H40a16,16,0,0,1-16-16V56A16,16,0,0,1,40,40H216A16,16,0,0,1,232,56Zm-68,56a8,8,0,0,0-3.41-6.55l-40-28A8,8,0,0,0,108,84v56a8,8,0,0,0,12.59,6.55l40-28A8,8,0,0,0,164,112Z'></path></svg>";

    private string VoorlichtingIcon = @"<svg xmlns='http://www.w3.org/2000/svg' width='67' height='67' fill='#ffffff' viewBox='0 0 256 256'><path d='M176,232a8,8,0,0,1-8,8H88a8,8,0,0,1,0-16h80A8,8,0,0,1,176,232Zm40-128a87.55,87.55,0,0,1-33.64,69.21A16.24,16.24,0,0,0,176,186v6a16,16,0,0,1-16,16H96a16,16,0,0,1-16-16v-6a16,16,0,0,0-6.23-12.66A87.59,87.59,0,0,1,40,104.49C39.74,56.83,78.26,17.14,125.88,16A88,88,0,0,1,216,104Zm-32.11-9.34a57.6,57.6,0,0,0-46.56-46.55,8,8,0,0,0-2.66,15.78c16.57,2.79,30.63,16.85,33.44,33.45A8,8,0,0,0,176,104a9,9,0,0,0,1.35-.11A8,8,0,0,0,183.89,94.66Z'></path></svg>";

    private string ChatIcon = @"<svg xmlns='http://www.w3.org/2000/svg' width='67' height='67' fill='#ffffff' viewBox='0 0 256 256'><path d='M232,64V192a16,16,0,0,1-16,16H83l-32.6,28.16-.09.07A15.89,15.89,0,0,1,40,240a16.05,16.05,0,0,1-6.79-1.52A15.84,15.84,0,0,1,24,224V64A16,16,0,0,1,40,48H216A16,16,0,0,1,232,64Z'></path></svg> ";


}