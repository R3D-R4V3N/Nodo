using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Rise.Client.Layout;

public partial class MainLayout : IAsyncDisposable
{
    private DotNetObjectReference<MainLayout>? _dotNetRef;
    private bool _canInstall;
    private bool _isPrompting;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JSRuntime.InvokeVoidAsync("pwaInstall.register", _dotNetRef);
        }
    }

    [JSInvokable]
    public Task OnInstallAvailable()
    {
        _canInstall = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task PromptInstallAsync()
    {
        if (_isPrompting)
        {
            return;
        }

        _isPrompting = true;
        var installed = await JSRuntime.InvokeAsync<bool>("pwaInstall.promptInstall");
        if (installed)
        {
            _canInstall = false;
        }

        _isPrompting = false;
        await InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        await Task.CompletedTask;
    }
}
