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
            ? "flex items-center justify-center w-10 h-10 rounded-full overflow-hidden border-2 border-white transition hover:scale-[1.02] hover:bg-white/10 md:w-full md:h-auto md:justify-start md:gap-3 md:rounded-2xl md:border-transparent md:px-4 md:py-3 md:hover:scale-100 md:hover:bg-neutral-100 md:text-neutral-600"
            : "flex items-center justify-center p-2.5 rounded-full transition hover:bg-white/20 md:w-full md:justify-start md:gap-3 md:rounded-2xl md:px-4 md:py-3 md:text-neutral-600 md:hover:bg-neutral-100 md:hover:text-[#127646]";

        var isActive = IsActive(href, match, additionalMatches);

        if (isProfile)
        {
            var activeClasses = isActive
                ? "bg-white text-[#127646] md:bg-[#127646] md:text-white"
                : "text-white md:text-neutral-600";

            return $"{baseClasses} {activeClasses}".Trim();
        }

        var stateClasses = isActive
            ? "bg-white text-[#127646] md:bg-[#127646] md:text-white md:hover:bg-[#127646] md:hover:text-white"
            : "text-white md:text-neutral-600";

        return $"{baseClasses} {stateClasses}".Trim();
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
