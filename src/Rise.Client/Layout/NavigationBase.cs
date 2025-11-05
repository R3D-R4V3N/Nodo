using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace Rise.Client.Layout;

public abstract class NavigationBase : ComponentBase, IDisposable
{
    private string _currentPath = "/";

    protected string CurrentPath => _currentPath;

    [Inject]
    protected NavigationManager NavigationManager { get; set; } = default!;

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _currentPath = GetCurrentPath(NavigationManager.Uri);
        NavigationManager.LocationChanged += HandleLocationChanged;
    }

    private void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        _currentPath = GetCurrentPath(args.Location);
        _ = InvokeAsync(StateHasChanged);
    }

    protected bool IsActive(string href, NavLinkMatch match = NavLinkMatch.Prefix, IReadOnlyList<string>? additionalMatches = null)
    {
        if (Match(_currentPath, href, match))
        {
            return true;
        }

        if (additionalMatches is not null)
        {
            foreach (var candidate in additionalMatches)
            {
                if (Match(_currentPath, candidate, NavLinkMatch.Prefix))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private string GetCurrentPath(string uri)
    {
        var relative = NavigationManager.ToBaseRelativePath(uri);
        return Normalize(relative);
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

    public virtual void Dispose()
    {
        NavigationManager.LocationChanged -= HandleLocationChanged;
    }
}
