using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Rise.Client.Profile.Models;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen
{
    private Task OpenHobbiesPicker()
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        _isPreferencePickerOpen = false;
        _preferencePickerMode = PreferencePickerMode.None;
        _preferencePickerSearch = string.Empty;
        _preferencePickerSelection.Clear();
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;

        _pickerSelection = _selectedHobbyIds.ToHashSet();
        _pickerSearch = string.Empty;
        _isPickerOpen = true;

        return Task.CompletedTask;
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
        else if (_pickerSelection.Count < HobbySelectionLimit)
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
        _selectedHobbyIds.Clear();
        foreach (var id in _pickerSelection)
        {
            _selectedHobbyIds.Add(id);
        }

        var updatedHobbies = _selectedHobbyIds
            .Select(CreateHobbyModel)
            .Where(h => h is not null)
            .Cast<ProfileHobbyModel>()
            .ToList();

        _model = _model with { Hobbies = updatedHobbies };
        _pickerSelection = _selectedHobbyIds.ToHashSet();

        _isPickerOpen = false;
        _pickerSearch = string.Empty;
        ToastService.ShowSuccess("Hobby's bijgewerkt");
    }

    private Task RemoveHobby(string id)
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        if (_selectedHobbyIds.Remove(id))
        {
            _pickerSelection.Remove(id);
            var updatedHobbies = _selectedHobbyIds
                .Select(CreateHobbyModel)
                .Where(h => h is not null)
                .Cast<ProfileHobbyModel>()
                .ToList();

            _model = _model with { Hobbies = updatedHobbies };
        }

        return Task.CompletedTask;
    }

    private Task OpenLikesPicker()
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        _isPickerOpen = false;
        _pickerSearch = string.Empty;

        _preferencePickerSelection = new HashSet<string>(_selectedLikeIds, StringComparer.OrdinalIgnoreCase);
        _preferencePickerMode = PreferencePickerMode.Likes;
        _preferencePickerSearch = string.Empty;
        _isPreferencePickerOpen = true;
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;

        return Task.CompletedTask;
    }

    private Task OpenDislikesPicker()
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        _isPickerOpen = false;
        _pickerSearch = string.Empty;

        _preferencePickerSelection = new HashSet<string>(_selectedDislikeIds, StringComparer.OrdinalIgnoreCase);
        _preferencePickerMode = PreferencePickerMode.Dislikes;
        _preferencePickerSearch = string.Empty;
        _isPreferencePickerOpen = true;
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;

        return Task.CompletedTask;
    }

    private Task OpenChatLinesPicker()
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        _isPickerOpen = false;
        _pickerSearch = string.Empty;
        _isPreferencePickerOpen = false;
        _preferencePickerSearch = string.Empty;
        _preferencePickerSelection.Clear();

        _chatLinePickerSelection = new HashSet<string>(_selectedChatLineIds, StringComparer.OrdinalIgnoreCase);
        _chatLinePickerSearch = string.Empty;
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        _isChatLinePickerOpen = true;

        return Task.CompletedTask;
    }

    private Task CloseChatLinePicker()
    {
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _chatLinePickerSelection.Clear();
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        return Task.CompletedTask;
    }

    private Task ToggleChatLinePickerSelection(string id)
    {
        if (_chatLinePickerSelection.Contains(id))
        {
            _chatLinePickerSelection.Remove(id);
        }
        else if (_chatLinePickerSelection.Count < ChatLineSelectionLimit)
        {
            _chatLinePickerSelection.Add(id);
        }

        return Task.CompletedTask;
    }

    private Task ClearChatLinePickerSelection()
    {
        _chatLinePickerSelection.Clear();
        return Task.CompletedTask;
    }

    private async Task ConfirmChatLinePickerSelection()
    {
        _selectedChatLineIds = OrderChatLineIds(_chatLinePickerSelection)
            .Take(ChatLineSelectionLimit)
            .ToList();

        _model = _model with { DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds) };

        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _chatLinePickerSelection.Clear();
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;

        ToastService.ShowSuccess("Favoriete chatzinnen bijgewerkt");
    }

    private Task UpdateChatLinePickerSearch(string value)
    {
        _chatLinePickerSearch = value;
        return Task.CompletedTask;
    }

    private Task UpdateNewChatLineText(string value)
    {
        _newChatLineText = value;

        if (!string.IsNullOrEmpty(_newChatLineError))
        {
            _newChatLineError = string.Empty;
        }

        return Task.CompletedTask;
    }

    private Task AddCustomChatLineAsync(string value)
    {
        if (_isAddingCustomChatLine)
        {
            return Task.CompletedTask;
        }

        _newChatLineError = string.Empty;

        var input = value?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(input))
        {
            _newChatLineError = "Typ een zin om toe te voegen.";
            return Task.CompletedTask;
        }

        if (input.Length > ChatLineTextMaxLength)
        {
            _newChatLineError = $"Zin mag maximaal {ChatLineTextMaxLength} tekens bevatten.";
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(input))
        {
            _newChatLineError = "Deze zin kan niet worden gebruikt.";
            return Task.CompletedTask;
        }

        var id = ResolveChatLineId(input);

        if (string.IsNullOrWhiteSpace(id))
        {
            _newChatLineError = "Deze zin kan niet worden gebruikt.";
            return Task.CompletedTask;
        }

        if (_chatLinePickerSelection.Contains(id))
        {
            _newChatLineError = "Deze zin staat al in je lijst.";
            return Task.CompletedTask;
        }

        if (_chatLinePickerSelection.Count >= ChatLineSelectionLimit)
        {
            _newChatLineError = $"Je kan maximaal {ChatLineSelectionLimit} zinnen kiezen.";
            return Task.CompletedTask;
        }

        _isAddingCustomChatLine = true;

        try
        {
            _chatLinePickerSelection.Add(id);
            _selectedChatLineIds = OrderChatLineIds(_chatLinePickerSelection)
                .Take(ChatLineSelectionLimit)
                .ToList();

            _model = _model with { DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds) };
            _newChatLineText = string.Empty;
            _newChatLineError = string.Empty;
        }
        finally
        {
            _isAddingCustomChatLine = false;
        }

        return Task.CompletedTask;
    }

    private Task RemoveChatLine(string id)
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        if (RemovePreference(_selectedChatLineIds, id))
        {
            _model = _model with { DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds) };
            _chatLinePickerSelection = new HashSet<string>(_selectedChatLineIds, StringComparer.OrdinalIgnoreCase);
            _newChatLineError = string.Empty;
        }

        return Task.CompletedTask;
    }

    private Task ClosePreferencePicker()
    {
        _isPreferencePickerOpen = false;
        _preferencePickerMode = PreferencePickerMode.None;
        _preferencePickerSearch = string.Empty;
        _preferencePickerSelection.Clear();
        return Task.CompletedTask;
    }

    private Task TogglePreferencePickerSelection(string id)
    {
        if (_preferencePickerSelection.Contains(id))
        {
            _preferencePickerSelection.Remove(id);
        }
        else if (_preferencePickerSelection.Count < PreferenceSelectionLimit)
        {
            _preferencePickerSelection.Add(id);
        }

        return Task.CompletedTask;
    }

    private Task ClearPreferencePickerSelection()
    {
        _preferencePickerSelection.Clear();
        return Task.CompletedTask;
    }

    private async Task ConfirmPreferencePickerSelection()
    {
        if (_preferencePickerMode == PreferencePickerMode.None)
        {
            _preferencePickerSelection.Clear();
            _isPreferencePickerOpen = false;
            _preferencePickerSearch = string.Empty;
            return;
        }

        var ordered = OrderPreferenceIds(_preferencePickerSelection)
            .Take(PreferenceSelectionLimit)
            .ToList();

        if (_preferencePickerMode == PreferencePickerMode.Likes)
        {
            _selectedLikeIds = ordered;
        }
        else
        {
            _selectedDislikeIds = ordered;
        }

        UpdateInterestsModel();

        var message = _preferencePickerMode == PreferencePickerMode.Likes
            ? "Wat ik leuk vind bijgewerkt"
            : "Wat ik minder leuk vind bijgewerkt";

        _isPreferencePickerOpen = false;
        _preferencePickerMode = PreferencePickerMode.None;
        _preferencePickerSearch = string.Empty;
        _preferencePickerSelection.Clear();

        ToastService.ShowSuccess(message);
    }

    private Task UpdatePreferencePickerSearch(string value)
    {
        _preferencePickerSearch = value;
        return Task.CompletedTask;
    }

    private Task RemoveLike(string id)
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        if (RemovePreference(_selectedLikeIds, id))
        {
            UpdateInterestsModel();
        }

        return Task.CompletedTask;
    }

    private Task RemoveDislike(string id)
    {
        if (!_isEditing)
        {
            return Task.CompletedTask;
        }

        if (RemovePreference(_selectedDislikeIds, id))
        {
            UpdateInterestsModel();
        }

        return Task.CompletedTask;
    }

    private Task UpdatePickerSearch(string value)
    {
        _pickerSearch = value;
        return Task.CompletedTask;
    }
}
