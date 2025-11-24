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
        await UpdateCurrentUserAsync();
    }

    private async void OnAuthStateChanged(Task<AuthenticationState> task)
    {
        await UpdateCurrentUserAsync();
        StateHasChanged();
    }

    private async Task UpdateCurrentUserAsync()
    {
        _isLoading = true;
        try
        {
            var currentUser = await UserContext.InitializeAsync();
            UserState.User = currentUser;
        }
        catch
        {
            UserState.User = null;
        }
        finally
        {
            _isLoading = false;
        }
    }

    public void Dispose()
    {
        AuthStateProvider.AuthenticationStateChanged -= OnAuthStateChanged;
    }
}