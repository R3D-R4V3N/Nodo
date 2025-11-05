using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Rise.Client.Layout;

public partial class SidebarNav : NavigationBase
{
    [Parameter]
    public EventCallback OnNavigate { get; set; }

    private string GetItemClasses(string href, NavLinkMatch match = NavLinkMatch.Prefix, params string[] additionalMatches)
    {
        const string baseClasses = "sidebar-link group flex items-center p-2 text-gray-600 rounded-lg transition hover:bg-gray-100 hover:text-gray-900 dark:text-gray-300 dark:hover:bg-gray-700 dark:hover:text-white";
        const string activeClasses = "bg-gray-100 text-gray-900 dark:bg-gray-700 dark:text-white sidebar-link--active";

        return IsActive(href, match, additionalMatches)
            ? $"{baseClasses} {activeClasses}"
            : baseClasses;
    }

    private string GetIconClasses(string href, NavLinkMatch match = NavLinkMatch.Prefix, params string[] additionalMatches)
    {
        const string baseClasses = "w-5 h-5 text-gray-500 transition duration-75 dark:text-gray-400 group-hover:text-gray-900 dark:group-hover:text-white";
        const string activeClasses = "text-gray-900 dark:text-white";

        return IsActive(href, match, additionalMatches)
            ? $"{baseClasses} {activeClasses}"
            : baseClasses;
    }

    private async Task HandleNavigation()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync();
        }
    }
}
