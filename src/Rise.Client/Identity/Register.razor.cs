using Ardalis.Result;
using Microsoft.AspNetCore.Components;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.Organizations;
using Serilog;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;

namespace Rise.Client.Identity;

public partial class Register
{
    [Inject] public required IAccountManager AccountManager { get; set; }

    [Inject] public required IHttpClientFactory HttpClientFactory { get; set; }

    private Result? _result;
    private AccountRequest.Register Model { get; set; } = new();

    private List<OrganizationDto.ListItem> Organizations { get; set; } = new();

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var client = HttpClientFactory.CreateClient("SecureApi");
            var response = await client.GetFromJsonAsync<Result<IReadOnlyCollection<OrganizationDto.ListItem>>>("/api/organizations");
            if (response?.IsSuccess == true && response.Value is not null)
            {
                Organizations = response.Value.OrderBy(o => o.Name).ToList();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Kon de organisaties niet laden.");
        }
    }

    public async Task RegisterUserAsync()
    {
        if (Model.OrganizationId is null)
        {
            _result = Result.Invalid(new ValidationError(nameof(Model.OrganizationId), "Selecteer een organisatie."));
            return;
        }

        var trimmedName = Model.FullName?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(trimmedName))
        {
            _result = Result.Invalid(new ValidationError(nameof(Model.FullName), "Naam mag niet leeg zijn."));
            return;
        }

        _result = await AccountManager.RegisterAsync(
            Model.Email!,
            Model.Password!,
            Model.ConfirmPassword!,
            trimmedName,
            Model.OrganizationId.Value);

    }
}
