using System;
using System.Collections.Generic;
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

        _isEditingPersonalInfo = false;
        _isEditingInterests = false;
        _isEditingChatLines = false;
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

    private Task NavigateBack()
    {
        NavigationManager.NavigateTo("/");
        return Task.CompletedTask;
    }

    private void BeginPersonalInfoEdit()
    {
        if (_isLoading || HasError)
        {
            return;
        }

        _draft = ProfileDraft.FromModel(_model);
        _isEditingPersonalInfo = true;
    }

    private void BeginInterestsEdit()
    {
        if (_isLoading || HasError)
        {
            return;
        }

        _pickerSelection = _selectedHobbyIds.ToHashSet();
        _initialHobbyIds = _selectedHobbyIds.ToHashSet();
        _initialLikeIds = _selectedLikeIds.ToList();
        _initialDislikeIds = _selectedDislikeIds.ToList();
        _preferencePickerSelection = new HashSet<string>(_selectedLikeIds, StringComparer.OrdinalIgnoreCase);
        _preferencePickerMode = PreferencePickerMode.None;
        _isPreferencePickerOpen = false;
        _preferencePickerSearch = string.Empty;

        _isEditingInterests = true;
    }

    private void BeginChatLinesEdit()
    {
        if (_isLoading || HasError)
        {
            return;
        }

        _initialChatLineIds = _selectedChatLineIds.ToList();
        _chatLinePickerSelection = new HashSet<string>(_selectedChatLineIds, StringComparer.OrdinalIgnoreCase);
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        _isAddingCustomChatLine = false;

        _isEditingChatLines = true;
    }

    private void CancelPersonalInfoEdit()
    {
        _draft = ProfileDraft.FromModel(_model);
        _isEditingPersonalInfo = false;
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

        _selectedLikeIds = OrderPreferenceIds(_initialLikeIds);
        _selectedDislikeIds = OrderPreferenceIds(_initialDislikeIds);
        _preferencePickerSelection = new HashSet<string>(_selectedLikeIds, StringComparer.OrdinalIgnoreCase);
        _preferencePickerMode = PreferencePickerMode.None;
        _isPreferencePickerOpen = false;
        _preferencePickerSearch = string.Empty;
        UpdateInterestsModel();

        _isEditingInterests = false;
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

    private enum ProfileSection
    {
        PersonalInfo,
        Interests,
        ChatLines
    }

    private List<string> GetModelHobbyIds() => _model.Hobbies
        .Select(h => h.Id)
        .Where(id => !string.IsNullOrWhiteSpace(id))
        .ToList();

    private (List<string> Likes, List<string> Dislikes) GetModelPreferenceIds()
    {
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

        return (OrderPreferenceIds(likeIds).Take(PreferenceSelectionLimit).ToList(),
            OrderPreferenceIds(dislikeIds).Take(PreferenceSelectionLimit).ToList());
    }

    private List<string> GetModelChatLineIds()
    {
        var chatLineIds = new List<string>();
        foreach (var line in _model.DefaultChatLines)
        {
            var id = ResolveChatLineId(line);
            if (!string.IsNullOrWhiteSpace(id))
            {
                chatLineIds.Add(id);
            }
        }

        return OrderChatLineIds(chatLineIds).Take(ChatLineSelectionLimit).ToList();
    }

    private UserRequest.UpdateCurrentUser BuildUpdateRequest(ProfileSection section)
    {
        var personalSource = section == ProfileSection.PersonalInfo ? _draft : ProfileDraft.FromModel(_model);
        var hobbySource = section == ProfileSection.Interests ? _selectedHobbyIds : GetModelHobbyIds();
        var (modelLikeIds, modelDislikeIds) = GetModelPreferenceIds();
        var likeSource = section == ProfileSection.Interests ? _selectedLikeIds : modelLikeIds;
        var dislikeSource = section == ProfileSection.Interests ? _selectedDislikeIds : modelDislikeIds;
        var chatLineSource = section == ProfileSection.ChatLines ? _selectedChatLineIds : GetModelChatLineIds();

        return new UserRequest.UpdateCurrentUser
        {
            FirstName = personalSource.FirstName ?? string.Empty,
            LastName = personalSource.LastName ?? string.Empty,
            Email = personalSource.Email ?? string.Empty,
            Biography = personalSource.Bio ?? string.Empty,
            AvatarUrl = personalSource.AvatarUrl ?? string.Empty,
            Gender = personalSource.Gender,
            Hobbies = hobbySource
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(x => new HobbyDto.EditProfile()
                {
                    Hobby = Enum.Parse<HobbyTypeDto>(x)
                })
                .ToList(),
            Sentiments = (likeSource ?? Enumerable.Empty<string>())
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Select(x => new SentimentDto.EditProfile
                {
                    Type = SentimentTypeDto.Like,
                    Category = Enum.Parse<SentimentCategoryTypeDto>(x)
                })
                .Concat((dislikeSource ?? Enumerable.Empty<string>())
                    .Where(id => !string.IsNullOrWhiteSpace(id))
                    .Select(x => new SentimentDto.EditProfile
                    {
                        Type = SentimentTypeDto.Dislike,
                        Category = Enum.Parse<SentimentCategoryTypeDto>(x)
                    }))
                .ToList(),
            DefaultChatLines = BuildChatLineTexts(chatLineSource)
                .Where(text => !string.IsNullOrWhiteSpace(text))
                .ToList()
        };
    }

    private async Task SaveSectionAsync(ProfileSection section)
    {
        if (_isLoading || HasError || _isSaving)
        {
            return;
        }

        if (section == ProfileSection.PersonalInfo && !_isEditingPersonalInfo)
        {
            return;
        }

        if (section == ProfileSection.Interests && !_isEditingInterests)
        {
            return;
        }

        if (section == ProfileSection.ChatLines && !_isEditingChatLines)
        {
            return;
        }

        var request = BuildUpdateRequest(section);

        var validationResult = await UpdateUserValidator.ValidateAsync(request);
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
            _isSaving = true;
            var result = await UserService.UpdateUserAsync(UserState?.User.AccountId, request);

            if (result.IsSuccess && result.Value.User is not null)
            {
                var updatedUser = result.Value.User;
                var memberSince = FormatMemberSince(updatedUser.CreatedAt);
                _model = ProfileModel.FromUser(updatedUser, memberSince);
                _draft = ProfileDraft.FromModel(_model);
                SyncSelectionFromModel();

                var message = section switch
                {
                    ProfileSection.PersonalInfo => "Persoonlijke gegevens opgeslagen",
                    ProfileSection.Interests => "Interesses en hobby's opgeslagen",
                    ProfileSection.ChatLines => "Standaardzinnen opgeslagen",
                    _ => "Wijziging opgeslagen"
                };

                ToastService.ShowSuccess(message);
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
            _isSaving = false;
        }
    }

    private Task SavePersonalInfoEdit() => SaveSectionAsync(ProfileSection.PersonalInfo);
    private Task SaveInterestsEdit() => SaveSectionAsync(ProfileSection.Interests);
    private Task SaveChatLinesEdit() => SaveSectionAsync(ProfileSection.ChatLines);

    private async Task OnAvatarChanged(InputFileChangeEventArgs args)
    {
        var file = args.File;
        if (file is null)
        {
            return;
        }

        if (!_isEditingPersonalInfo)
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
            _model = _model with { AvatarUrl = dataUrl };
        }
        catch
        {
            // Ignore failures and keep existing avatar.
        }
    }
}
