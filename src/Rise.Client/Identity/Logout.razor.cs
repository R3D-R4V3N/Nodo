using Microsoft.AspNetCore.Components;
using Rise.Client.Offline;
using Rise.Client.State;

namespace Rise.Client.Identity;

public partial class Logout
{
    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required ICacheService CacheService { get; set; }
    protected override async Task OnInitializedAsync()
    {
        if (await AccountManager.CheckAuthenticatedAsync())
        {
            await AccountManager.LogoutAsync();
            await CacheService.ClearCacheAsync(default);
        }

        // forceload or else scripts are missing?
        Navigation.NavigateTo("/", forceLoad: true);
    }
}