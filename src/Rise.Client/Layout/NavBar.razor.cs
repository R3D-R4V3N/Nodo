using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Rise.Client.State;
using Rise.Shared.Users;

namespace Rise.Client.Layout;

public partial class NavBar : IDisposable
{
    private string _currentPath = "/";

    [Inject]
    public UserState UserState { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _currentPath = GetCurrentPath(NavigationManager.Uri);
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    private string GetNavItemClasses(string href, NavLinkMatch match = NavLinkMatch.Prefix, bool isProfile = false, params string[] additionalMatches)
    {
        var baseClasses = isProfile
            ? "flex h-12 w-12 items-center justify-center rounded-full overflow-hidden border-2 border-white transition hover:scale-110 focus:outline-none focus-visible:ring-2 focus-visible:ring-white/60 lg:h-16 lg:w-16"
            : "flex h-12 w-12 items-center justify-center rounded-full transition hover:bg-white/20 focus:outline-none focus-visible:ring-2 focus-visible:ring-white/60 lg:h-auto lg:w-full lg:justify-start lg:gap-3 lg:rounded-2xl lg:px-4 lg:py-3";

        var isActive = IsActive(href, match, additionalMatches);

        if (isProfile)
        {
            return isActive
                ? $"{baseClasses} bg-white text-[#127646]"
                : $"{baseClasses}";
        }

        var textClass = isActive ? "text-[#127646]" : "text-white";
        var backgroundClass = isActive ? "bg-white" : "lg:bg-white/10";
        var activeDesktopClass = isActive ? "lg:bg-white lg:text-[#127646]" : "lg:text-white";

        return $"{baseClasses} {backgroundClass} {textClass} {activeDesktopClass}".Trim();
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

    public void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }
}
