using Microsoft.AspNetCore.Components;
using Rise.Client.Registrations;
using Rise.Client.State;
using Rise.Shared.Registrations;
using System.Collections.Generic;
using System.Linq;

namespace Rise.Client.Supervisors;

public partial class Registrations
{
    [Inject] public required IRegistrationClient RegistrationClient { get; set; }
    [Inject] public required UserState UserState { get; set; }

    private readonly List<RegistrationDto.Pending> _registrations = [];
    private bool _isLoading = true;
    private bool _isBusy;
    private string? _error;
    private string? _statusMessage;

    private string? CurrentAccountId => UserState.User?.AccountId;

    protected override async Task OnInitializedAsync()
    {
        await LoadRegistrationsAsync();
    }

    private async Task LoadRegistrationsAsync()
    {
        _isLoading = true;
        _error = null;
        _statusMessage = null;

        var result = await RegistrationClient.GetPendingRegistrationsAsync();

        if (!result.IsSuccess)
        {
            _error = result.Errors.FirstOrDefault() ?? "Kon de aanvragen niet laden.";
            _registrations.Clear();
        }
        else
        {
            _registrations.Clear();
            _registrations.AddRange(result.Value.Registrations);
        }

        _isLoading = false;
    }

    private async Task AssignToMeAsync(RegistrationDto.Pending registration)
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        _error = null;
        _statusMessage = null;

        var result = await RegistrationClient.AssignSupervisorAsync(registration.Id);

        if (!result.IsSuccess)
        {
            _error = result.Errors.FirstOrDefault() ?? "Kon de aanvraag niet koppelen.";
        }
        else
        {
            var updated = result.Value.Registration;
            var index = _registrations.FindIndex(r => r.Id == updated.Id);
            if (index >= 0)
            {
                _registrations[index] = updated;
            }
            _statusMessage = $"Aanvraag van {updated.FullName} is gekoppeld aan jou.";
        }

        _isBusy = false;
    }

    private async Task ApproveAsync(RegistrationDto.Pending registration)
    {
        if (_isBusy)
        {
            return;
        }

        _isBusy = true;
        _error = null;
        _statusMessage = null;

        var result = await RegistrationClient.ApproveRegistrationAsync(registration.Id);

        if (!result.IsSuccess)
        {
            _error = result.Errors.FirstOrDefault() ?? "Kon de aanvraag niet goedkeuren.";
        }
        else
        {
            _registrations.RemoveAll(r => r.Id == registration.Id);
            _statusMessage = $"Aanvraag van {registration.FullName} is goedgekeurd.";
        }

        _isBusy = false;
    }
}
