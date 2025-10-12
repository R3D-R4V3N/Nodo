using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Identity.Accounts;

namespace Rise.Client.Identity;

public partial class Login
{
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    private AccountRequest.Login Model = new();
    private Result _result = new();
    private bool _hasRedirected;
    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [CascadingParameter] private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        if (_hasRedirected || AuthenticationStateTask is null)
        {
            return;
        }

        var state = await AuthenticationStateTask;

        if (state.User.Identity?.IsAuthenticated == true)
        {
            _hasRedirected = true;
            var destination = string.IsNullOrEmpty(ReturnUrl) ? "/homepage" : ReturnUrl;
            Navigation.NavigateTo(destination, true);
        }
    }

    public async Task LoginUser()
    {
        _result = await AccountManager.LoginAsync(Model.Email!, Model.Password!);

        if (_result.IsSuccess)
        {
            _hasRedirected = true;
            var destination = string.IsNullOrEmpty(ReturnUrl) ? "/homepage" : ReturnUrl;
            Navigation.NavigateTo(destination, true);
        }
    }
}