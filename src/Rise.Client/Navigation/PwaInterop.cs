using Microsoft.JSInterop;

namespace Rise.Client.Navigation;

/// <summary>
/// Provides JS interop helpers for detecting whether the app is running as an installed PWA
/// and for applying the shared redirect logic from JavaScript.
/// </summary>
public sealed class PwaInterop : IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private IJSObjectReference? _module;

    public PwaInterop(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    /// <summary>
    /// Returns true when the app is running in standalone/PWA mode.
    /// </summary>
    public async ValueTask<bool> IsPwaAsync()
    {
        var module = await GetModuleAsync();
        return await module.InvokeAsync<bool>("isPWA");
    }

    /// <summary>
    /// Invokes the shared JS redirect routine so the browser-side logic stays consistent.
    /// </summary>
    public async ValueTask EnforceClientRedirectAsync()
    {
        var module = await GetModuleAsync();
        await module.InvokeVoidAsync("enforcePwaRouting");
    }

    private async ValueTask<IJSObjectReference> GetModuleAsync()
    {
        _module ??= await _jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/pwa-navigation.js");
        return _module;
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
