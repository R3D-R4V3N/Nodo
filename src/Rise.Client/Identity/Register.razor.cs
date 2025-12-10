using Blazored.Toast.Services;
using Ardalis.Result;
using Microsoft.AspNetCore.Components;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;
using Serilog;
using System.Linq;

namespace Rise.Client.Identity;

public partial class Register
{
    [Inject] public required IAccountManager AccountManager { get; set; }
    [Inject] public required IOrganizationService OrganizationService { get; set; }
    [Inject] public required IToastService ToastService { get; set; }

    private Result? _result;
    private readonly AccountRequest.Register Model = new();
    private List<OrganizationDto.Summary> _organizations = [];
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
                var loadError = result.Errors.FirstOrDefault() ?? "Kon organisaties niet laden.";
                ToastService.ShowError(loadError);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Could not load organizations for registration.");
            ToastService.ShowError("Kon organisaties niet laden.");
        }
    }

    public async Task RegisterUserAsync()
    {
        _result = null;
        _isSubmitting = true;
        try
        {
            _result = await AccountManager.RegisterAsync(Model);

            if (_result is { IsSuccess: false })
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
                    ToastService.ShowError("Registratie is mislukt.");
                }
            }
        }
        finally
        {
            _isSubmitting = false;
        }
    }
}
