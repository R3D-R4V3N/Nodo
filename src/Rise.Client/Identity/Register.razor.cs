using System.Linq;
using Microsoft.AspNetCore.Components;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;

namespace Rise.Client.Identity;

public partial class Register
{
    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required IOrganizationService OrganizationService { get; set; }

    private Result? _result;
    private AccountRequest.Register Model { get; set; } = new();
    private IReadOnlyCollection<OrganizationDto.Summary> _organizations = Array.Empty<OrganizationDto.Summary>();
    private string? _organizationError;

    protected override async Task OnInitializedAsync()
    {
        var organizationsResult = await OrganizationService.GetOrganizationsAsync();

        if (organizationsResult.IsSuccess)
        {
            _organizations = organizationsResult.Value.Organizations;
        }
        else
        {
            _organizationError = organizationsResult.Errors.FirstOrDefault() ?? "Kon de lijst met organisaties niet laden.";
        }
    }

    public async Task RegisterUserAsync()
    {
        if (!Model.OrganizationId.HasValue)
        {
            _result = Result.Invalid(new ValidationError(nameof(Model.OrganizationId), "Selecteer een organisatie."));
            return;
        }

        _result = await AccountManager.RegisterAsync(
            Model.Email!,
            Model.Password!,
            Model.ConfirmPassword!,
            Model.OrganizationId.Value);

    }
}

