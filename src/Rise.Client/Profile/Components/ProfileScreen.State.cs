using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using FluentValidation;
using Microsoft.AspNetCore.Components.Forms;
using Rise.Client.Profile.Models;
using Rise.Shared.Hobbies;
using Rise.Shared.Sentiments;
using Rise.Shared.Users;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen
{
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var currentUser = UserState.User!;

            var memberSince = FormatMemberSince(currentUser.CreatedAt);
            _model = ProfileModel.FromUser(currentUser, memberSince);
            _draft = ProfileDraft.FromModel(_model);
            SyncSelectionFromModel();
        }
        catch
        {
            _loadError = "Er ging iets mis bij het laden van je profiel.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void SyncSelectionFromModel()
    {
        _selectedHobbyIds.Clear();
        foreach (var hobby in _model.Hobbies)
        {
            if (!string.IsNullOrWhiteSpace(hobby.Id))
            {
                _selectedHobbyIds.Add(hobby.Id);
            }
        }

        _pickerSelection = _selectedHobbyIds.ToHashSet();
        _initialHobbyIds = _selectedHobbyIds.ToHashSet();

        _customPreferenceOptions.Clear();
        _customChatLineOptions.Clear();

        var likeIds = new List<string>();
        var dislikeIds = new List<string>();

        foreach (var interest in _model.Interests)
        {
            if (!string.IsNullOrWhiteSpace(interest.Like))
            {
                var likeId = ResolvePreferenceId(interest.Like);
                if (!string.IsNullOrWhiteSpace(likeId))
                {
                    likeIds.Add(likeId);
                }
            }

            if (!string.IsNullOrWhiteSpace(interest.Dislike))
            {
                var dislikeId = ResolvePreferenceId(interest.Dislike);
                if (!string.IsNullOrWhiteSpace(dislikeId))
                {
                    dislikeIds.Add(dislikeId);
                }
            }
        }

        _selectedLikeIds = OrderPreferenceIds(likeIds).Take(PreferenceSelectionLimit).ToList();
        _selectedDislikeIds = OrderPreferenceIds(dislikeIds).Take(PreferenceSelectionLimit).ToList();
        _initialLikeIds = _selectedLikeIds.ToList();
        _initialDislikeIds = _selectedDislikeIds.ToList();

        _preferencePickerSelection = new HashSet<string>(_selectedLikeIds, StringComparer.OrdinalIgnoreCase);
        _preferencePickerMode = PreferencePickerMode.None;
        _isPreferencePickerOpen = false;
        _preferencePickerSearch = string.Empty;

        var chatLineIds = new List<string>();
        foreach (var line in _model.DefaultChatLines)
        {
            var id = ResolveChatLineId(line);
            if (!string.IsNullOrWhiteSpace(id))
            {
                chatLineIds.Add(id);
            }
        }

        _selectedChatLineIds = OrderChatLineIds(chatLineIds).Take(ChatLineSelectionLimit).ToList();
        _initialChatLineIds = _selectedChatLineIds.ToList();
        _chatLinePickerSelection = new HashSet<string>(_selectedChatLineIds, StringComparer.OrdinalIgnoreCase);
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        _isAddingCustomChatLine = false;

        _model = _model with { DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds) };

        UpdateInterestsModel();
    }

    private void ApplyUpdatedUser(UserDto.CurrentUser updatedUser)
    {
        var memberSince = FormatMemberSince(updatedUser.CreatedAt);
        _model = ProfileModel.FromUser(updatedUser, memberSince);
        _draft = ProfileDraft.FromModel(_model);
        SyncSelectionFromModel();
        UserState.User = updatedUser;
    }

    private static string FormatMemberSince(DateTime createdAt)
    {
        if (createdAt == default)
        {
            return "Actief sinds onbekend";
        }

        var culture = System.Globalization.CultureInfo.GetCultureInfo("nl-BE");
        var formatted = createdAt.ToString("MMMM yyyy", culture);
        formatted = culture.TextInfo.ToTitleCase(formatted);
        return $"Actief sinds {formatted}";
    }

    private bool CanStartEditingSection()
    {
        if (!CanEditProfile)
        {
            return false;
        }

        if (IsLoading || HasError)
        {
            return false;
        }

        if (AnySectionEditing)
        {
            ToastService.ShowInfo("Rond eerst je huidige bewerking af.");
            return false;
        }

        return true;
    }

    private Task NavigateBack()
    {
        NavigationManager.NavigateTo("/");
        return Task.CompletedTask;
    }

    private void BeginPersonalInfoEdit()
    {
        if (!CanStartEditingSection())
        {
            return;
        }

        _draft = ProfileDraft.FromModel(_model);
        _isEditingPersonalInfo = true;
    }

    private void CancelPersonalInfoEdit()
    {
        _draft = ProfileDraft.FromModel(_model);
        _isEditingPersonalInfo = false;
    }

    private async Task SavePersonalInfoAsync()
    {
        if (!CanEditProfile || !_isEditingPersonalInfo || _isSavingPersonalInfo)
        {
            return;
        }

        var request = new UserRequest.UpdatePersonalInfo
        {
            FirstName = _draft.FirstName ?? string.Empty,
            LastName = _draft.LastName ?? string.Empty,
            Email = _draft.Email ?? string.Empty,
            Biography = _draft.Bio ?? string.Empty,
            AvatarUrl = _draft.AvatarUrl ?? string.Empty,
            Gender = _draft.Gender
        };

        var validationResult = await UpdatePersonalInfoValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors.Select(e => e.ErrorMessage).Distinct())
            {
                ToastService.ShowError(error);
            }

            return;
        }

        try
        {
            _isSavingPersonalInfo = true;
            var accountId = UserState?.User?.AccountId;
            if (string.IsNullOrWhiteSpace(accountId))
            {
                ToastService.ShowError("Kan geen wijzigingen opslaan zonder account-id.");
                return;
            }

            var result = await UserService.UpdatePersonalInfoAsync(accountId, request);

            if (result.IsSuccess && result.Value.User is not null)
            {
                ApplyUpdatedUser(result.Value.User);
                _isEditingPersonalInfo = false;
                ToastService.ShowSuccess("Persoonlijke gegevens opgeslagen");
            }
            else
            {
                List<string> errors = [.. result.ValidationErrors.Select(e => e.ErrorMessage), .. result.Errors];
                if (errors.Count == 0)
                {
                    errors.Add("Opslaan is mislukt.");
                }

                foreach (var err in errors)
                {
                    ToastService.ShowError(err);
                }
            }
        }
        catch
        {
            ToastService.ShowError("Opslaan is mislukt.");
        }
        finally
        {
            _isSavingPersonalInfo = false;
        }
    }

    private void BeginInterestsEdit()
    {
        if (!CanStartEditingSection())
        {
            return;
        }

        _initialHobbyIds = _selectedHobbyIds.ToHashSet();
        _initialLikeIds = _selectedLikeIds.ToList();
        _initialDislikeIds = _selectedDislikeIds.ToList();
        _isEditingInterests = true;
    }

    private void CancelInterestsEdit()
    {
        _selectedHobbyIds.Clear();
        foreach (var id in _initialHobbyIds)
        {
            _selectedHobbyIds.Add(id);
        }

        var restoredHobbies = _selectedHobbyIds
            .Select(CreateHobbyModel)
            .Where(h => h is not null)
            .Cast<ProfileHobbyModel>()
            .ToList();

        _model = _model with { Hobbies = restoredHobbies };
        _pickerSelection = _selectedHobbyIds.ToHashSet();
        _isPickerOpen = false;
        _pickerSearch = string.Empty;

        _selectedLikeIds = OrderPreferenceIds(_initialLikeIds);
        _selectedDislikeIds = OrderPreferenceIds(_initialDislikeIds);
        _preferencePickerSelection = new HashSet<string>(_selectedLikeIds, StringComparer.OrdinalIgnoreCase);
        _preferencePickerMode = PreferencePickerMode.None;
        _isPreferencePickerOpen = false;
        _preferencePickerSearch = string.Empty;

        UpdateInterestsModel();

        _isEditingInterests = false;
    }

    private async Task SaveInterestsAsync()
    {
        if (!CanEditProfile || !_isEditingInterests || _isSavingInterests)
        {
            return;
        }

        var request = new UserRequest.UpdateInterests
        {
            Hobbies = _selectedHobbyIds
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .Select(x => new HobbyDto.EditProfile()
                        {
                            Hobby = Enum.Parse<HobbyTypeDto>(x)
                        })
                        .ToList(),
            Sentiments = (_selectedLikeIds ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(x => new SentimentDto.EditProfile
                {
                    Type = SentimentTypeDto.Like,
                    Category = Enum.Parse<SentimentCategoryTypeDto>(x)
                })
                .Concat((_selectedDislikeIds ?? Enumerable.Empty<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(x => new SentimentDto.EditProfile
                    {
                        Type = SentimentTypeDto.Dislike,
                        Category = Enum.Parse<SentimentCategoryTypeDto>(x)
                    })
                ).ToList()
        };

        var validationResult = await UpdateInterestsValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors.Select(e => e.ErrorMessage).Distinct())
            {
                ToastService.ShowError(error);
            }

            return;
        }

        try
        {
            _isSavingInterests = true;
            var accountId = UserState?.User?.AccountId;
            if (string.IsNullOrWhiteSpace(accountId))
            {
                ToastService.ShowError("Kan geen wijzigingen opslaan zonder account-id.");
                return;
            }

            var result = await UserService.UpdateInterestsAsync(accountId, request);

            if (result.IsSuccess && result.Value.User is not null)
            {
                ApplyUpdatedUser(result.Value.User);
                _isEditingInterests = false;
                ToastService.ShowSuccess("Interesses bijgewerkt");
            }
            else
            {
                List<string> errors = [.. result.ValidationErrors.Select(e => e.ErrorMessage), .. result.Errors];
                if (errors.Count == 0)
                {
                    errors.Add("Opslaan is mislukt.");
                }

                foreach (var err in errors)
                {
                    ToastService.ShowError(err);
                }
            }
        }
        catch
        {
            ToastService.ShowError("Opslaan is mislukt.");
        }
        finally
        {
            _isSavingInterests = false;
        }
    }

    private void BeginChatLinesEdit()
    {
        if (!CanStartEditingSection())
        {
            return;
        }

        _initialChatLineIds = _selectedChatLineIds.ToList();
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        _isEditingChatLines = true;
    }

    private void CancelChatLinesEdit()
    {
        _selectedChatLineIds = OrderChatLineIds(_initialChatLineIds);
        _chatLinePickerSelection = new HashSet<string>(_selectedChatLineIds, StringComparer.OrdinalIgnoreCase);
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        _isAddingCustomChatLine = false;
        _model = _model with { DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds) };
        _isEditingChatLines = false;
    }

    private async Task SaveChatLinesAsync()
    {
        if (!CanEditProfile || !_isEditingChatLines || _isSavingChatLines)
        {
            return;
        }

        var request = new UserRequest.UpdateDefaultChatLines
        {
            DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList()
        };

        var validationResult = await UpdateChatLinesValidator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            foreach (var error in validationResult.Errors.Select(e => e.ErrorMessage).Distinct())
            {
                ToastService.ShowError(error);
            }

            return;
        }

        try
        {
            _isSavingChatLines = true;
            var accountId = UserState?.User?.AccountId;
            if (string.IsNullOrWhiteSpace(accountId))
            {
                ToastService.ShowError("Kan geen wijzigingen opslaan zonder account-id.");
                return;
            }

            var result = await UserService.UpdateDefaultChatLinesAsync(accountId, request);

            if (result.IsSuccess && result.Value.User is not null)
            {
                ApplyUpdatedUser(result.Value.User);
                _isEditingChatLines = false;
                ToastService.ShowSuccess("Standaardzinnen opgeslagen");
            }
            else
            {
                List<string> errors = [.. result.ValidationErrors.Select(e => e.ErrorMessage), .. result.Errors];
                if (errors.Count == 0)
                {
                    errors.Add("Opslaan is mislukt.");
                }

                foreach (var err in errors)
                {
                    ToastService.ShowError(err);
                }
            }
        }
        catch
        {
            ToastService.ShowError("Opslaan is mislukt.");
        }
        finally
        {
            _isSavingChatLines = false;
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
            using var stream = file.OpenReadStream(maxAllowedSize: 300 * 1024);
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            var base64 = Convert.ToBase64String(memory.ToArray());
            var dataUrl = $"data:{file.ContentType};base64,{base64}";
            _draft.AvatarUrl = dataUrl;
            if (!_isEditingPersonalInfo)
            {
                _model = _model with { AvatarUrl = dataUrl };
            }
        }
        catch
        {
            // Ignore failures and keep existing avatar.
        }
    }
}
