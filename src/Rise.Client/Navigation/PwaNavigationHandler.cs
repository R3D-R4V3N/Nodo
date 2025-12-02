using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.Extensions.Logging;

namespace Rise.Client.Navigation;

/// <summary>
/// Listens to navigation changes and enforces the PWA/standalone routing rules.
/// </summary>
public sealed class PwaNavigationHandler : IAsyncDisposable
{
    private readonly NavigationManager _navigationManager;
    private readonly PwaInterop _pwaInterop;
    private readonly ILogger<PwaNavigationHandler> _logger;
    private bool _initialized;
    private bool _isHandlingNavigation;

    public PwaNavigationHandler(
        NavigationManager navigationManager,
        PwaInterop pwaInterop,
        ILogger<PwaNavigationHandler> logger)
    {
        _navigationManager = navigationManager;
        _pwaInterop = pwaInterop;
        _logger = logger;
    }

    /// <summary>
    /// Hook into NavigationManager and immediately apply the redirect rules on startup.
    /// </summary>
    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        _navigationManager.LocationChanged += HandleLocationChanged;
        _initialized = true;

        await ApplyRulesAsync(_navigationManager.Uri);
    }

    private async void HandleLocationChanged(object? sender, LocationChangedEventArgs args)
    {
        try
        {
            await ApplyRulesAsync(args.Location);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enforce PWA navigation for {Location}", args.Location);
        }
    }

    private async Task ApplyRulesAsync(string absoluteUri)
    {
        if (_isHandlingNavigation)
        {
            return;
        }

        _isHandlingNavigation = true;

        try
        {
            // Ask JS whether we are running inside an installed PWA.
            var isPwa = await _pwaInterop.IsPwaAsync();
            var relativePath = NormalizePath(_navigationManager.ToBaseRelativePath(absoluteUri));

            // Non-PWA visitors are always confined to /pwa.
            if (!isPwa)
            {
                if (!IsPwaRoute(relativePath))
                {
                    _navigationManager.NavigateTo("/pwa", forceLoad: true);
                }

                return;
            }

            // PWA users should never stay on /pwa.
            if (IsPwaRoute(relativePath))
            {
                _navigationManager.NavigateTo("/homepage", forceLoad: true);
            }
        }
        finally
        {
            _isHandlingNavigation = false;
        }
    }

    private static bool IsPwaRoute(string relativePath)
    {
        return string.Equals(relativePath, "/pwa", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string relativePath)
    {
        var path = relativePath ?? string.Empty;
        var trimmed = path.Split('?', '#')[0];

        if (string.IsNullOrWhiteSpace(trimmed))
        {
            return "/";
        }

        return trimmed.StartsWith('/') ? trimmed : $"/{trimmed}";
    }

    public async ValueTask DisposeAsync()
    {
        _navigationManager.LocationChanged -= HandleLocationChanged;
        await _pwaInterop.DisposeAsync();
    }
}
