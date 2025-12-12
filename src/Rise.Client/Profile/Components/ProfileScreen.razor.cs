using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using FluentValidation;
using Rise.Client.Profile.Models;
using Rise.Client.State;
using Rise.Client.Users;
using Rise.Shared.Assets;
using Rise.Shared.BlobStorage;
using Rise.Shared.Common;
using Rise.Shared.Hobbies;
using Rise.Shared.Sentiments;
using Rise.Shared.Users;
using Blazored.Toast.Services;

namespace Rise.Client.Profile.Components;

[Authorize]
public partial class ProfileScreen : ComponentBase
{
    private static readonly IReadOnlyList<HobbyOption> _hobbyOptions = Enum.GetValues<HobbyTypeDto>()
        .Select(x =>
        {
            var (Name, Emoji) = HobbyDto.TranslateEnumToText(x);
            return new HobbyOption(x.ToString(), Name, Emoji);
        }).ToList();

    private static readonly IReadOnlyList<PreferenceOption> _preferenceOptions = Enum.GetValues<SentimentCategoryTypeDto>()
        .Select(x =>
        {
            var (Name, Emoji) = SentimentDto.TranslateEnumToText(x);
            return new PreferenceOption(x.ToString(), Name, Emoji);
        }).ToList();
        
    private static readonly IReadOnlyDictionary<string, PreferenceOption> _preferenceOptionsById =
        _preferenceOptions.ToDictionary(option => option.Id, option => option, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, string> _preferenceIdByName =
        _preferenceOptions.ToDictionary(option => option.Name, option => option.Id, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, int> _preferenceOrderById =
        _preferenceOptions
            .Select((option, index) => new { option.Id, index })
            .ToDictionary(entry => entry.Id, entry => entry.index, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<PreferenceOption> _chatLineOptions = ChatLineDefaults.Options;

    private static readonly IReadOnlyDictionary<string, PreferenceOption> _chatLineOptionsById =
        _chatLineOptions.ToDictionary(option => option.Id, option => option, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, string> _chatLineIdByName =
        _chatLineOptions.ToDictionary(option => option.Name, option => option.Id, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, int> _chatLineOrderById =
        _chatLineOptions
            .Select((option, index) => new { option.Id, index })
            .ToDictionary(entry => entry.Id, entry => entry.index, StringComparer.OrdinalIgnoreCase);

    private const int HobbySelectionLimit = 3;
    private const int PreferenceSelectionLimit = 5;
    private const int ChatLineSelectionLimit = 5;
    private const int ChatLineTextMaxLength = 150;
    private const long MaxAvatarSize = 2 * 1024 * 1024;

    private ProfileModel _model = new();
    private ProfileDraft _draft;

    private readonly HashSet<string> _selectedHobbyIds = new();
    private HashSet<string> _initialHobbyIds = new();
    private HashSet<string> _pickerSelection = new();

    private List<string> _selectedLikeIds = new();
    private List<string> _selectedDislikeIds = new();
    private List<string> _initialLikeIds = new();
    private List<string> _initialDislikeIds = new();
    private readonly Dictionary<string, string> _customPreferenceOptions = new(StringComparer.OrdinalIgnoreCase);

    private List<string> _selectedChatLineIds = new();
    private List<string> _initialChatLineIds = new();
    private HashSet<string> _chatLinePickerSelection = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, string> _customChatLineOptions = new(StringComparer.OrdinalIgnoreCase);

    private bool _isEditing;
    private bool _isSaving;
    private bool _isLoading = true;
    private string? _loadError;

    private bool _isPickerOpen;
    private string _pickerSearch = string.Empty;

    private HashSet<string> _preferencePickerSelection = new(StringComparer.OrdinalIgnoreCase);
    private PreferencePickerMode _preferencePickerMode = PreferencePickerMode.None;
    private bool _isPreferencePickerOpen;
    private string _preferencePickerSearch = string.Empty;

    private bool _isChatLinePickerOpen;
    private string _chatLinePickerSearch = string.Empty;
    private string _newChatLineText = string.Empty;
    private string _newChatLineError = string.Empty;
    private bool _isAddingCustomChatLine;

    private enum PreferencePickerMode
    {
        None,
        Likes,
        Dislikes
    }

    public ProfileScreen()
    {
        _draft = ProfileDraft.FromModel(_model);
        SyncSelectionFromModel();
    }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IUserService UserService { get; set; } = default!;
    [Inject] private UserState UserState { get; set; } = default!;
    [Inject] private IValidator<UserRequest.UpdateCurrentUser> UpdateUserValidator { get; set; } = default!;

    [Inject] private IToastService ToastService { get; set; } = default!;
    
    private string GetAvatarSource()
    {
        if (_draft.AvatarBlob?.Base64Data is string base64)
            return base64;

        return _draft.AvatarUrl ?? DefaultImages.Profile;
    }

    private bool IsEditing => _isEditing;
    private bool IsLoading => _isLoading;
    private bool HasError => !string.IsNullOrWhiteSpace(_loadError);
    private string? ErrorMessage => _loadError;
    private IReadOnlyList<ProfileHobbyModel> Hobbies => _model.Hobbies;
    private IReadOnlyList<HobbyOption> HobbyOptions => _hobbyOptions;
    private IReadOnlyList<PreferenceOption> PreferenceOptions => _preferenceOptions;
    private IReadOnlyList<PreferenceOption> ChatLineOptions
    {
        get
        {
            if (_customChatLineOptions.Count == 0)
            {
                return _chatLineOptions;
            }

            var combined = new List<PreferenceOption>(_chatLineOptions.Count + _customChatLineOptions.Count);
            combined.AddRange(_chatLineOptions);

            foreach (var option in _customChatLineOptions
                .Where(entry => !_chatLineOptionsById.ContainsKey(entry.Key))
                .OrderBy(entry => entry.Value, StringComparer.OrdinalIgnoreCase))
            {
                combined.Add(new PreferenceOption(option.Key, option.Value));
            }

            return combined;
        }
    }
    private IReadOnlyList<PreferenceChip> LikeChips => BuildPreferenceChips(_selectedLikeIds);
    private IReadOnlyList<PreferenceChip> DislikeChips => BuildPreferenceChips(_selectedDislikeIds);
    private IReadOnlyList<PreferenceChip> ChatLineChips => BuildChatLineChips(_selectedChatLineIds);
    private IReadOnlyCollection<string> PickerSelection => _pickerSelection;
    private string PickerSearch => _pickerSearch;
    private bool IsPickerOpen => _isPickerOpen;
    private int MaxHobbies => HobbySelectionLimit;
    private int MaxLikes => PreferenceSelectionLimit;
    private int MaxDislikes => PreferenceSelectionLimit;
    private int MaxPreferences => PreferenceSelectionLimit;
    private int MaxChatLines => ChatLineSelectionLimit;
    private IReadOnlyCollection<string> PreferencePickerSelection => _preferencePickerSelection;
    private string PreferencePickerSearch => _preferencePickerSearch;
    private bool IsPreferencePickerOpen => _isPreferencePickerOpen;
    private bool IsDislikePicker => _preferencePickerMode == PreferencePickerMode.Dislikes;
    private bool IsChatLinePickerOpen => _isChatLinePickerOpen;
    private IReadOnlyCollection<string> ChatLinePickerSelection => _chatLinePickerSelection;
    private string ChatLinePickerSearch => _chatLinePickerSearch;
    private string NewChatLineText => _newChatLineText;
    private string NewChatLineError => _newChatLineError;
    private bool IsAddingCustomChatLine => _isAddingCustomChatLine;
    private bool IsCustomChatLineLimitReached => _chatLinePickerSelection.Count >= ChatLineSelectionLimit;
    private int CustomChatLineMaxLength => ChatLineTextMaxLength;
    private string BirthDayDisplay => FormatBirthDay(_draft.BirthDay);
    private string PreferencePickerTitle => _preferencePickerMode switch
    {
        PreferencePickerMode.Likes => "Kies wat je leuk vindt",
        PreferencePickerMode.Dislikes => "Kies wat je minder leuk vindt",
        _ => string.Empty
    };
    private string PreferencePickerPlaceholder => _preferencePickerMode switch
    {
        PreferencePickerMode.Likes => "Zoek iets dat je leuk vindt…",
        PreferencePickerMode.Dislikes => "Zoek iets dat je niet fijn vindt…",
        _ => "Zoek..."
    };
    private string DisplayName => string.IsNullOrWhiteSpace(CurrentName) ? "Jouw Naam" : CurrentName;
    private string CurrentName => _isEditing 
        ? $"{_draft.FirstName} {_draft.LastName}" 
        : $"{_model.FirstName} {_model.LastName}";
}
