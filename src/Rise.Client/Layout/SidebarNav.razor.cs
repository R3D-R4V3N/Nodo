using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Rise.Client.Layout;

public partial class SidebarNav : IDisposable
{
    private string _currentPath = "/";

    [Parameter]
    public EventCallback OnNavigate { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _currentPath = GetCurrentPath(NavigationManager.Uri);
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    private string GetItemClasses(string href, NavLinkMatch match = NavLinkMatch.Prefix, params string[]? additionalMatches)
    {
        const string baseClasses = "sidebar-link group flex items-center p-2 text-gray-600 rounded-lg transition hover:bg-gray-100 hover:text-gray-900 dark:text-gray-300 dark:hover:bg-gray-700 dark:hover:text-white";
        const string activeClasses = "bg-gray-100 text-gray-900 dark:bg-gray-700 dark:text-white sidebar-link--active";

        return IsActive(href, match, additionalMatches ?? Array.Empty<string>())
            ? $"{baseClasses} {activeClasses}"
            : baseClasses;
    }

    private string GetIconClasses(string href, NavLinkMatch match = NavLinkMatch.Prefix, params string[]? additionalMatches)
    {
        const string baseClasses = "w-5 h-5 text-gray-500 transition duration-75 dark:text-gray-400 group-hover:text-gray-900 dark:group-hover:text-white";
        const string activeClasses = "text-gray-900 dark:text-white";

        return IsActive(href, match, additionalMatches ?? Array.Empty<string>())
            ? $"{baseClasses} {activeClasses}"
            : baseClasses;
    }

    private bool IsActive(string href, NavLinkMatch match, IReadOnlyList<string> additionalMatches)
    {
        if (Match(_currentPath, href, match))
        {
            return true;
        }

        foreach (var candidate in additionalMatches)
        {
            if (Match(_currentPath, candidate, NavLinkMatch.Prefix))
            {
                return true;
            }
        }

        return false;
    }

    private static bool Match(string currentPath, string href, NavLinkMatch match)
    {
        var normalizedCurrent = Normalize(currentPath);
        var normalizedHref = Normalize(href);

        if (match == NavLinkMatch.All)
        {
            return string.Equals(normalizedCurrent, normalizedHref, StringComparison.OrdinalIgnoreCase);
        }

        if (normalizedHref == "/")
        {
            return normalizedCurrent == "/";
        }

        return normalizedCurrent.StartsWith(normalizedHref, StringComparison.OrdinalIgnoreCase);
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _currentPath = GetCurrentPath(args.Location);
        _ = InvokeAsync(StateHasChanged);
    }

    private string GetCurrentPath(string uri)
    {
        var relative = NavigationManager.ToBaseRelativePath(uri);
        return Normalize(relative);
    }

    private static string Normalize(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            return "/";
        }

        var trimmed = path;
        var queryIndex = trimmed.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex >= 0)
        {
            trimmed = trimmed[..queryIndex];
        }

        if (!trimmed.StartsWith('/'))
        {
            trimmed = "/" + trimmed;
        }

        if (trimmed.Length > 1 && trimmed.EndsWith('/'))
        {
            trimmed = trimmed.TrimEnd('/');
        }

        return trimmed;
    }

    private async Task HandleNavigation()
    {
        if (OnNavigate.HasDelegate)
        {
            await OnNavigate.InvokeAsync();
        }
    }

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }
}
