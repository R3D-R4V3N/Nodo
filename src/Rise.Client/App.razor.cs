using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.State;
using Rise.Client.Users;

namespace Rise.Client;
public partial class App : IDisposable
{
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] public UserContextService UserContext { get; set; }
    [Inject] public UserState UserState { get; set; } = default!;
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
        }
        StateHasChanged();
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
        }
        StateHasChanged();
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
    }
}