using Ardalis.Result;
using Microsoft.AspNetCore.Components;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;
using Serilog;

namespace Rise.Client.Identity;

public partial class Register
{
    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required IOrganizationService OrganizationService { get; set; }

    private Result? _result;
    private readonly AccountRequest.Register Model = new();
    private List<OrganizationDto.Summary> _organizations = [];
    private string? _loadError;
    private bool _isSubmitting;

    protected override async Task OnInitializedAsync()
    {
        await LoadOrganizationsAsync();
    }

    private async Task LoadOrganizationsAsync()
    {
        try
        {
            var result = await OrganizationService.GetOrganizationsAsync();
            if (result is { IsSuccess: true })
            {
                _organizations = result.Value.Organizations.ToList();
            }
            else
            {
                _loadError = result.Errors.FirstOrDefault() ?? "Kon organisaties niet laden.";
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not load organizations for registration.");
            _loadError = "Kon organisaties niet laden.";
        }
    }

    public async Task RegisterUserAsync()
    {
        _result = null;
        _isSubmitting = true;
        try
        {
            _result = await AccountManager.RegisterAsync(Model);
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
