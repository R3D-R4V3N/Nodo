using System;
using System.Collections.Generic;
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
    private AccountRequest.Register Model { get; set; } = new()
    {
        FirstName = string.Empty,
        LastName = string.Empty,
        OrganizationId = 0
    };
    private IEnumerable<OrganizationDto.Index> Organizations { get; set; } = Array.Empty<OrganizationDto.Index>();
    private string? _organizationLoadError;
    private bool _isLoadingOrganizations = true;

    protected override async Task OnInitializedAsync()
    {
        var organizationResult = await OrganizationService.GetAllAsync();
        if (organizationResult.IsSuccess)
        {
            Organizations = organizationResult.Value.Organizations;
            if (!Organizations.Any())
            {
                _organizationLoadError = "Er zijn momenteel geen organisaties beschikbaar.";
            }
        }
        else
        {
            _organizationLoadError = organizationResult.Errors.FirstOrDefault()
                ?? "Kon de lijst met organisaties niet laden.";
        }

        _isLoadingOrganizations = false;
    }

    public async Task RegisterUserAsync()
    {
        _result = await AccountManager.RegisterAsync(
            Model.Email!,
            Model.Password!,
            Model.ConfirmPassword!,
            Model.FirstName!,
            Model.LastName!,
            Model.OrganizationId);

    }
}
