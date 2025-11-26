using Microsoft.AspNetCore.Components;
using Rise.Client.State;

namespace Rise.Client.Identity;

public partial class Logout
{
    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    protected override async Task OnInitializedAsync()
    {
        if (await AccountManager.CheckAuthenticatedAsync())
        {
            await AccountManager.LogoutAsync();
        }
        Navigation.NavigateTo("/");
    }
}