using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;

namespace Rise.Client.Home.Pages;
public partial class Index
{
    [Inject] public required NavigationManager Navigation { get; set; }
    [Inject] public required AuthenticationStateProvider AuthenticationStateProvider { get; set; }
    private bool _isLoading = true;
    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();

        if (authenticationState.User.Identity?.IsAuthenticated == true)
        {
            Navigation.NavigateTo("/homepage");
        }

        _isLoading = false;
    }
}