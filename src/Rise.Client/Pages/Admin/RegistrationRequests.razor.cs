using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Rise.Client.Registrations;
using Rise.Shared.Registrations;

namespace Rise.Client.Pages.Admin;

public partial class RegistrationRequestsPage : ComponentBase
{
    [Inject] public IRegistrationRequestClient RegistrationRequestClient { get; set; } = default!;

    protected readonly List<RegistrationRequestResponse.PendingItem> _requests = [];
    protected readonly Dictionary<int, int?> _selectedSupervisors = new();
    protected bool _isLoading = true;
    protected bool _isProcessing;
    protected string? _errorMessage;
    protected string? _successMessage;

    protected override async Task OnInitializedAsync()
    {
        await LoadRequestsAsync();
    }

    protected async Task LoadRequestsAsync()
    {
        _isLoading = true;
        _errorMessage = null;
        _successMessage = null;

        var result = await RegistrationRequestClient.GetPendingAsync();

        _requests.Clear();
        _selectedSupervisors.Clear();

        if (result.IsSuccess)
        {
            _requests.AddRange(result.Value);
        }
        else
        {
            _errorMessage = result.Errors.FirstOrDefault() ?? "Kon de aanvragen niet laden.";
        }

        _isLoading = false;
        StateHasChanged();
    }

    protected void OnSupervisorChanged(int requestId, ChangeEventArgs args)
    {
        if (int.TryParse(args.Value?.ToString(), out var supervisorId))
        {
            _selectedSupervisors[requestId] = supervisorId;
        }
        else
        {
            _selectedSupervisors[requestId] = null;
        }
    }

    protected async Task ApproveAsync(int requestId)
    {
        if (_isProcessing)
        {
            return;
        }

        _errorMessage = null;
        _successMessage = null;

        if (!_selectedSupervisors.TryGetValue(requestId, out var supervisorId) || supervisorId is null)
        {
            _errorMessage = "Selecteer een supervisor voordat je goedkeurt.";
            return;
        }

        _isProcessing = true;
        var result = await RegistrationRequestClient.ApproveAsync(
            requestId,
            new RegistrationRequestRequest.Approve { SupervisorId = supervisorId.Value });
        _isProcessing = false;

        if (!result.IsSuccess)
        {
            _errorMessage = result.Errors.FirstOrDefault() ?? "Kon de aanvraag niet goedkeuren.";
            return;
        }

        var removed = _requests.RemoveAll(r => r.Id == requestId);
        _selectedSupervisors.Remove(requestId);

        _successMessage = removed > 0
            ? "Registratieaanvraag goedgekeurd."
            : "Registratieaanvraag verwerkt.";
    }
}
