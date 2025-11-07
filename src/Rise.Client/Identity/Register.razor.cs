using Microsoft.AspNetCore.Components;
using Rise.Client.Registrations;
using System.Linq;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;

namespace Rise.Client.Identity;

public partial class Register
{
    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required IRegistrationClient RegistrationClient { get; set; }

    private Result? _result;
    private string? _loadError;
    private List<OrganizationDto.Summary> Organizations { get; set; } = [];
    private AccountRequest.Register Model { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        var organizationResult = await RegistrationClient.GetOrganizationsAsync();

        if (!organizationResult.IsSuccess)
        {
            _loadError = organizationResult.Errors.FirstOrDefault() ?? "Kon de organisaties niet laden.";
            return;
        }

        Organizations = organizationResult.Value.Organizations;
    }

    public async Task RegisterUserAsync()
    {
        _result = await AccountManager.RegisterAsync(Model);

        if (_result.IsSuccess)
        {
            Model = new AccountRequest.Register();
        }
    }
}