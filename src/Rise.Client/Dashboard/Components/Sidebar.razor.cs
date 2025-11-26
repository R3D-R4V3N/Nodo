using Microsoft.AspNetCore.Components;

namespace Rise.Client.Dashboard.Components;
public partial class Sidebar
{
    [Inject] NavigationManager Nav { get; set; } = default!;
    void Logout()
    {
        Nav.NavigateTo("/logout", forceLoad: true);
    }
}