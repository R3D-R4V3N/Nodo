using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using Rise.Client.State;
using Rise.Client.Users;
using Rise.Client.Validators;
using Rise.Shared.Validators;

namespace Rise.Client;
public partial class App : IDisposable
{
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] public UserContextService UserContext { get; set; }
    [Inject] public UserState UserState { get; set; } = default!;
    [Inject] public NavigationManager NavigationManager { get; set; } = default!;
    [Inject] public IJSRuntime JsRuntime { get; set; } = default!;

    private bool _isMobileBrowser;
    private bool _isStandalone;
    private bool _pwaStatusLoaded;
    private bool _isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        AuthStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
        _isLoading = true;
        try
        {
            await UserContext.SetUserStateAsync();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async void OnAuthStateChanged(Task<AuthenticationState> task)
    {
        _isLoading = true;
        try
        {
            await UserContext.UpdateUserStateAsync();
        }
        finally
        {
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task HandleNavigationAsync(NavigationContext context)
    {
        await EnsurePwaStatusAsync();

        if (ShouldRedirectToPwa(context.Path))
        {
            NavigationManager.NavigateTo("/pwa", replace: true);
        }
    }

    private async Task EnsurePwaStatusAsync()
    {
        if (_pwaStatusLoaded)
        {
            var latestStatus = await GetPwaStatusAsync();
            _isMobileBrowser = latestStatus.IsMobile;
            _isStandalone = latestStatus.IsStandalone;
            return;
        }

        var status = await GetPwaStatusAsync();
        _isMobileBrowser = status.IsMobile;
        _isStandalone = status.IsStandalone;
        _pwaStatusLoaded = true;
    }

    private async Task<PwaStatus> GetPwaStatusAsync()
    {
        try
        {
            return await JsRuntime.InvokeAsync<PwaStatus>("pwaStatus.getStatus");
        }
        catch
        {
            return new PwaStatus(false, true);
        }
    }

    private bool ShouldRedirectToPwa(string path)
    {
        if (!_isMobileBrowser || _isStandalone)
        {
            return false;
        }

        var normalizedPath = NormalizePath(path);
        return !string.Equals(normalizedPath, "/pwa", StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizePath(string path)
    {
        var normalized = string.IsNullOrWhiteSpace(path) ? "/" : path;

        if (!normalized.StartsWith('/'))
        {
            normalized = "/" + normalized;
        }

        var queryIndex = normalized.IndexOf('?', StringComparison.Ordinal);
        if (queryIndex >= 0)
        {
            normalized = normalized[..queryIndex];
        }

        if (normalized.Length > 1 && normalized.EndsWith('/'))
        {
            normalized = normalized.TrimEnd('/');
        }

        return normalized;
    }

    private record PwaStatus(bool IsMobile, bool IsStandalone);

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
    }
}
