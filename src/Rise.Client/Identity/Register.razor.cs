using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Rise.Client.Organizations;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;

namespace Rise.Client.Identity;

public partial class Register
{
    [Inject] public required IAccountManager AccountManager { get; set; }

    [Inject] public required IOrganizationService OrganizationService { get; set; }

    private Result? _result;
    private AccountRequest.Register Model { get; set; } = new();
    private List<OrganizationResponse.ListItem> _organizations = [];
    private string? _organizationError;

    protected override async Task OnInitializedAsync()
    {
        var organizationsResult = await OrganizationService.GetOrganizationsAsync();

        if (organizationsResult is { IsSuccess: true, Value: var value })
        {
            _organizations = value;
        }
        else
        {
            _organizationError = organizationsResult?.Errors.FirstOrDefault()
                ?? "Kon de lijst met organisaties niet laden.";
        }
    }

    public async Task RegisterUserAsync()
    {
        _result = await AccountManager.RegisterAsync(Model);

    }
}