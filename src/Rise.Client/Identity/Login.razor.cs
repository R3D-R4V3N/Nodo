using Ardalis.Result;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Rise.Shared.Identity.Accounts;
using System.Threading.Tasks;

namespace Rise.Client.Identity;

public partial class Login
{
    [SupplyParameterFromQuery] private string? ReturnUrl { get; set; }

    private AccountRequest.Login Model = new();
    private Result _result = new();
    private bool _hasNavigated;

    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }

    [CascadingParameter]
    private Task<AuthenticationState>? AuthenticationStateTask { get; set; }

    protected override async Task OnParametersSetAsync()
    {
        await RedirectAuthenticatedUserAsync();
    }

    public async Task LoginUser()
    {
        _result = await AccountManager.LoginAsync(Model.Email!, Model.Password!);

        if (!_result.IsSuccess)
        {
            return;
        }

        _hasNavigated = true;

        if (!string.IsNullOrEmpty(ReturnUrl))
        {
            Navigation.NavigateTo(ReturnUrl, forceLoad: true);
            return;
        }

        Navigation.NavigateTo("/homepage", forceLoad: true);
    }

    private async Task RedirectAuthenticatedUserAsync()
    {
        if (_hasNavigated || AuthenticationStateTask is null)
        {
            return;
        }

        var authState = await AuthenticationStateTask;
        if (authState.User.Identity?.IsAuthenticated == true)
        {
            _hasNavigated = true;
            Navigation.NavigateTo("/homepage", forceLoad: true);
        }
    }
}
