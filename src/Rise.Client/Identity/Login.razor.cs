using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Identity;
using Rise.Shared.Identity.Accounts;
using System.Linq;

namespace Rise.Client.Identity;

public partial class Login
{
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    private AccountRequest.Login Model = new();
    private Result _result = new();

    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    [Inject] public required IToastService ToastService { get; set; }

    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authenticationState.User.Identity?.IsAuthenticated == true)
        {
            if (authenticationState.User.IsInRole(AppRoles.Supervisor) || authenticationState.User.IsInRole(AppRoles.Administrator))
            {
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                Navigation.NavigateTo("/homepage");
            }
        }
    }

    public async Task LoginUser()
    {
        _result = await AccountManager.LoginAsync(Model.Email!, Model.Password!);

        if (!_result.IsSuccess)
        {
            if (_result.Errors?.Any() == true)
            {
                foreach (var error in _result.Errors)
                {
                    ToastService.ShowError(error);
                }
            }
            else
            {
                ToastService.ShowError("Inloggen mislukt.");
            }
        }
        else if (!string.IsNullOrEmpty(ReturnUrl))
        {
            Navigation.NavigateTo(ReturnUrl);
        }
        else if (_result.IsSuccess)
        {
            var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
            if (authenticationState.User.IsInRole(AppRoles.Supervisor) || authenticationState.User.IsInRole(AppRoles.Administrator))
            {
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                Navigation.NavigateTo("/homepage");
            }
        }
    }
}
