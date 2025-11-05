using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;

namespace Rise.Client.Layout;

public partial class MainLayout : LayoutComponentBase
{
    private bool _isSidebarOpen;

    private string GetSidebarClasses()
    {
        var baseClasses = "fixed top-0 left-0 z-40 w-64 h-screen transition-transform";
        var translateClass = _isSidebarOpen ? "translate-x-0" : "-translate-x-full";

        return $"{baseClasses} {translateClass} sm:translate-x-0";
    }

    private string GetMainClasses()
    {
        const string baseClasses = "relative transition-[margin-left] duration-300 ease-in-out";
        const string desktopMargin = "sm:ml-64";

        return $"{baseClasses} {desktopMargin}";
    }

    private void ToggleSidebar() => _isSidebarOpen = !_isSidebarOpen;

    private void CloseSidebar() => _isSidebarOpen = false;

    private Task HandleSidebarNavigate()
    {
        CloseSidebar();
        return Task.CompletedTask;
    }
}
