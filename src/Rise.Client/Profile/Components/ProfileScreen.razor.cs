using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Ardalis.Result;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.Extensions.Logging;
using Rise.Client.Profile.Models;
using Rise.Shared.Profile;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen : ComponentBase, IDisposable
{
    private ProfileModel _model = ProfileModel.CreateDefault();
    private ProfileDraft _draft;
    private IReadOnlyList<InterestOption> _interestOptions = Array.Empty<InterestOption>();
    private int _maxInterests = ProfileCatalog.MaxInterestCount;

    private readonly HashSet<string> _selectedInterestIds = new();
    private HashSet<string> _pickerSelection = new();

    private bool _isEditing;
    private bool _isPickerOpen;
    private string _pickerSearch = string.Empty;
    private bool _isInitialized;
    private bool _isSaving;
    private string? _errorMessage;

    private bool _isToastVisible;
    private string _toastMessage = string.Empty;
    private CancellationTokenSource? _toastCts;

    public ProfileScreen()
    {
        _draft = ProfileDraft.FromModel(_model);
    }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IProfileService ProfileService { get; set; } = default!;
    [Inject] private ILogger<ProfileScreen> Logger { get; set; } = default!;

    private bool IsEditing => _isEditing;
    private bool IsPickerOpen => _isPickerOpen;
    private IReadOnlyList<InterestOption> InterestOptions => _interestOptions;
    private IReadOnlyList<InterestOption> SelectedInterests => InterestOptions
        .Where(i => _selectedInterestIds.Contains(i.Id))
        .ToList();
    private string DisplayName
    {
        get
        {
            var value = _isEditing ? _draft.FullName : _model.DisplayName;
            return string.IsNullOrWhiteSpace(value) ? "Jouw Naam" : value;
        }
    }
    private int MaxInterests => _maxInterests;

    protected override async Task OnInitializedAsync()
    {
        await LoadProfileAsync();
    }

    private Task NavigateBack()
    {
        NavigationManager.NavigateTo("/");
        return Task.CompletedTask;
    }

    private async Task LoadProfileAsync()
    {
        try
        {
            var result = await ProfileService.GetAsync();
            if (!result.IsSuccess || result.Value?.Profile is null)
            {
                _errorMessage = ExtractMessage(result, "Kon profiel niet laden.");
            }
            else
            {
                ApplyResponse(result.Value);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fout bij het laden van het profiel");
            _errorMessage = "Kon profiel niet laden.";
        }
        finally
        {
            _isInitialized = true;
            await InvokeAsync(StateHasChanged);
        }
    }

    private void ApplyResponse(ProfileResponse.Envelope envelope)
    {
        _model = ProfileModel.FromResponse(envelope.Profile);
        _draft = ProfileDraft.FromModel(_model);
        _interestOptions = envelope.AvailableInterests
            .Select(i => new InterestOption(i.Id, i.Name, i.Emoji))
            .ToList();
        _maxInterests = envelope.MaxInterestCount;
        ResetSelectionFromModel();
        _errorMessage = null;
    }

    private void ResetSelectionFromModel()
    {
        _selectedInterestIds.Clear();
        foreach (var id in _model.Interests)
        {
            _selectedInterestIds.Add(id);
        }

        _pickerSelection = _selectedInterestIds.ToHashSet();
    }

    private void BeginEdit()
    {
        if (!_isInitialized || _isSaving)
        {
            return;
        }

        _draft = ProfileDraft.FromModel(_model);
        _pickerSelection = _selectedInterestIds.ToHashSet();
        _isEditing = true;
    }

    private void CancelEdit()
    {
        _draft = ProfileDraft.FromModel(_model);
        ResetSelectionFromModel();
        _isEditing = false;
    }

    private async Task SaveEdit()
    {
        if (_isSaving)
        {
            return;
        }

        _isSaving = true;

        try
        {
            var request = _draft.ToUpdateRequest(_model, _selectedInterestIds);
            var success = await PersistAsync(request, "Wijzigingen opgeslagen", "Opslaan mislukt", revertSelectionOnFailure: false);
            if (success)
            {
                _isEditing = false;
            }
        }
        finally
        {
            _isSaving = false;
        }
    }

    private async Task OnAvatarChanged(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file is null)
        {
            return;
        }

        try
        {
            using var stream = file.OpenReadStream(maxAllowedSize: 5 * 1024 * 1024);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            var base64 = Convert.ToBase64String(memory.ToArray());
            var dataUrl = $"data:{file.ContentType};base64,{base64}";
            _draft.AvatarUrl = dataUrl;
            if (!_isEditing)
            {
                var request = _model.ToUpdateRequest(_selectedInterestIds) with { AvatarUrl = dataUrl };
                await PersistAsync(request, "Profielfoto bijgewerkt", "Profielfoto bijwerken mislukt", revertSelectionOnFailure: true);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fout bij het aanpassen van de avatar");
        }
    }

    private void OpenInterestsPicker()
    {
        if (!_isInitialized || _isSaving)
        {
            return;
        }

        _pickerSelection = _selectedInterestIds.ToHashSet();
        _pickerSearch = string.Empty;
        _isPickerOpen = true;
    }

    private Task ClosePicker()
    {
        _isPickerOpen = false;
        _pickerSearch = string.Empty;
        return Task.CompletedTask;
    }

    private Task TogglePickerSelection(string id)
    {
        if (_pickerSelection.Contains(id))
        {
            _pickerSelection.Remove(id);
        }
        else if (_pickerSelection.Count < MaxInterests)
        {
            _pickerSelection.Add(id);
        }

        return Task.CompletedTask;
    }

    private Task ClearPickerSelection()
    {
        _pickerSelection.Clear();
        return Task.CompletedTask;
    }

    private async Task ConfirmPickerSelection()
    {
        _selectedInterestIds.Clear();
        foreach (var id in _pickerSelection)
        {
            _selectedInterestIds.Add(id);
        }

        _isPickerOpen = false;
        _pickerSearch = string.Empty;

        if (_isEditing)
        {
            await ShowToastAsync("Interesses bijgewerkt");
            return;
        }

        if (_isSaving)
        {
            return;
        }

        var request = _model.ToUpdateRequest(_selectedInterestIds);
        await PersistAsync(request, "Interesses bijgewerkt", "Bijwerken van interesses mislukt", revertSelectionOnFailure: true);
    }

    private async Task RemoveInterest(string id)
    {
        if (!_selectedInterestIds.Remove(id))
        {
            return;
        }

        _pickerSelection.Remove(id);

        if (_isEditing || _isSaving)
        {
            return;
        }

        var request = _model.ToUpdateRequest(_selectedInterestIds);
        await PersistAsync(request, "Interesse verwijderd", "Bijwerken van interesses mislukt", revertSelectionOnFailure: true);
    }

    private Task UpdatePickerSearch(string value)
    {
        _pickerSearch = value;
        return Task.CompletedTask;
    }

    private async Task<bool> PersistAsync(ProfileRequest.UpdateProfile request, string successMessage, string failureMessage, bool revertSelectionOnFailure)
    {
        try
        {
            var result = await ProfileService.UpdateAsync(request);
            if (!result.IsSuccess || result.Value?.Profile is null)
            {
                if (revertSelectionOnFailure)
                {
                    ResetSelectionFromModel();
                }

                await ShowToastAsync(ExtractMessage(result, failureMessage));
                return false;
            }

            ApplyResponse(result.Value);
            await ShowToastAsync(successMessage);
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Fout bij het bijwerken van het profiel");
            if (revertSelectionOnFailure)
            {
                ResetSelectionFromModel();
            }

            await ShowToastAsync(failureMessage);
            return false;
        }
    }

    private static string ExtractMessage(Result<ProfileResponse.Envelope> result, string fallback)
    {
        if (result.ValidationErrors?.Any() == true)
        {
            return result.ValidationErrors.First().ErrorMessage;
        }

        if (result.Errors?.Any() == true)
        {
            return result.Errors.First();
        }

        return result.Status switch
        {
            ResultStatus.Unauthorized => "Je bent niet aangemeld.",
            ResultStatus.Forbidden => "Je hebt geen toegang tot dit profiel.",
            _ => fallback
        };
    }

    private async Task ShowToastAsync(string message)
    {
        _toastCts?.Cancel();
        _toastCts?.Dispose();
        _toastCts = new CancellationTokenSource();
        _toastMessage = message;
        _isToastVisible = true;
        await InvokeAsync(StateHasChanged);

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(1.4), _toastCts.Token);
        }
        catch (TaskCanceledException)
        {
            return;
        }

        _isToastVisible = false;
        await InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        _toastCts?.Cancel();
        _toastCts?.Dispose();
    }
}
