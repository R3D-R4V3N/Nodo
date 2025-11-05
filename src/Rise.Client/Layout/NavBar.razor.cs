using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Rise.Client.State;
using Rise.Shared.Users;

namespace Rise.Client.Layout;

public partial class NavBar : NavigationBase
{
    [Inject]
    public UserState UserState { get; set; }

    private string GetNavItemClasses(string href, NavLinkMatch match = NavLinkMatch.Prefix, bool isProfile = false, params string[] additionalMatches)
    {
        var baseClasses = isProfile
            ? "w-10 h-10 rounded-full overflow-hidden border-2 border-white hover:scale-110 transition"
            : "p-2.5 rounded-full hover:bg-white/20 transition";

        var isActive = IsActive(href, match, additionalMatches);

        if (isProfile)
        {
            return isActive
                ? $"{baseClasses} bg-white"
                : $"{baseClasses}";
        }

        var textClass = isActive ? "text-[#127646]" : "text-white";
        var backgroundClass = isActive ? "bg-white" : string.Empty;

        return $"{baseClasses} {backgroundClass} {textClass}".Trim();
    }
}
