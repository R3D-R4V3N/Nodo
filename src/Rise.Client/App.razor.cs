using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Client.Users;
using Rise.Shared.Users;

namespace Rise.Client;
public partial class App
{
    [Inject] public AuthenticationStateProvider AuthStateProvider { get; set; }
    [Inject] public UserContextService UserContext { get; set; }
    private UserDto.CurrentUser? _currentUser;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            AuthStateProvider.AuthenticationStateChanged += OnAuthStateChanged;
            _currentUser = await UserContext.InitializeAsync();
        }
        catch (Exception)
        {
            _currentUser = null;
        }
    }
    private async void OnAuthStateChanged(Task<AuthenticationState> task)
    {
        try
        {
            _currentUser = await UserContext.InitializeAsync();
        }
        catch (Exception)
        {
            _currentUser = null;
        }
        finally
        {
            StateHasChanged();
        }
    }
}