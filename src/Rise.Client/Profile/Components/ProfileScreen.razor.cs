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
using Rise.Client.Profile.Models;
using Rise.Client.Users;
using Rise.Shared.Common;
using Rise.Shared.Users;
using Ardalis.Result;

namespace Rise.Client.Profile.Components;

[Authorize]
public partial class ProfileScreen : ComponentBase, IDisposable
{
    private static readonly IReadOnlyList<HobbyOption> _hobbyOptions = new List<HobbyOption>
    {
        new("Swimming", "Zwemmen", "🏊"),
        new("Football", "Voetbal", "⚽"),
        new("Rugby", "Rugby", "🏉"),
        new("Basketball", "Basketbal", "🏀"),
        new("Gaming", "Gamen", "🎮"),
        new("Cooking", "Koken", "🍳"),
        new("Baking", "Bakken", "🧁"),
        new("Hiking", "Wandelen in de natuur", "🥾"),
        new("Cycling", "Fietsen", "🚴"),
        new("Drawing", "Tekenen", "✏️"),
        new("Painting", "Schilderen", "🎨"),
        new("MusicMaking", "Muziek maken", "🎶"),
        new("Singing", "Zingen", "🎤"),
        new("Dancing", "Dansen", "💃"),
        new("Reading", "Lezen", "📚"),
        new("Gardening", "Tuinieren", "🌱"),
        new("Fishing", "Vissen", "🎣"),
        new("Camping", "Kamperen", "🏕️"),
        new("Photography", "Fotografie", "📸"),
        new("Crafting", "Knutselen", "✂️"),
        new("Sewing", "Naaien", "🧵"),
        new("Knitting", "Breien", "🧶"),
        new("Woodworking", "Houtbewerking", "🪚"),
        new("Pottery", "Keramiek", "🏺"),
        new("Writing", "Verhalen schrijven", "✍️"),
        new("Birdwatching", "Vogels spotten", "🐦"),
        new("ModelBuilding", "Modelbouw", "🧱"),
        new("Chess", "Schaken", "♟️"),
        new("BoardGames", "Bordspellen", "🎲"),
        new("Puzzles", "Puzzels leggen", "🧩"),
        new("CardGames", "Kaartspellen", "🃏"),
        new("Running", "Hardlopen", "🏃"),
        new("Yoga", "Yoga", "🧘"),
        new("Pilates", "Pilates", "🤸"),
        new("Skating", "Skeeleren", "⛸️"),
        new("Bouldering", "Boulderen", "🧗"),
    };

    private static readonly IReadOnlyList<PreferenceOption> _preferenceOptions = new List<PreferenceOption>
    {
        new("travel-adventures", "Reizen", "✈️"),
        new("city-trips", "Stedentrips", "🏙️"),
        new("beach-days", "Stranddagen", "🏖️"),
        new("mountain-views", "Bergen bewonderen", "🏔️"),
        new("shopping-sprees", "Shoppen", "🛍️"),
        new("market-visits", "Markten bezoeken", "🛒"),
        new("cozy-cafes", "Gezellige cafeetjes", "☕"),
        new("dining-out", "Uit eten gaan", "🍽️"),
        new("street-food", "Straatvoedsel proeven", "🌮"),
        new("new-flavours", "Nieuwe smaken proberen", "🧂"),
        new("sweet-treats", "Zoete desserts", "🍰"),
        new("savoury-snacks", "Hartige snacks", "🥨"),
        new("spicy-dishes", "Pittig eten", "🌶️"),
        new("fresh-salads", "Frisse salades", "🥗"),
        new("seasonal-soups", "Seizoenssoepen", "🍲"),
        new("fruity-moments", "Vers fruit", "🍓"),
        new("chocolate-moments", "Chocolade", "🍫"),
        new("cheese-boards", "Kaasplankjes", "🧀"),
        new("coffee-breaks", "Koffie momenten", "☕"),
        new("tea-time", "Theepauzes", "🍵"),
        new("smoothie-bar", "Smoothies", "🥤"),
        new("juice-stands", "Verse sappen", "🧃"),
        new("breakfast-dates", "Uitgebreide ontbijtjes", "🥐"),
        new("brunch-plans", "Weekendbrunch", "🥞"),
        new("picnic-plans", "Picknicken", "🧺"),
        new("food-trucks", "Foodtrucks", "🚚"),
        new("farmers-markets", "Boerenmarkten", "🌻"),
        new("road-trips", "Roadtrips", "🚗"),
        new("train-journeys", "Treinreizen", "🚆"),
        new("ferry-rides", "Boottochtjes", "⛴️"),
        new("wellness-days", "Wellness dagen", "💆"),
        new("spa-relax", "Spa bezoeken", "🧖"),
        new("sauna-evenings", "Saunabezoek", "🧖‍♂️"),
        new("cinema-nights", "Bioscoopavonden", "🎬"),
        new("series-marathons", "Series bingewatchen", "📺"),
        new("romantic-movies", "Romantische films", "💞"),
        new("action-movies", "Actiefilms", "💥"),
        new("horror-movies", "Horrorfilms", "👻"),
        new("documentaries", "Documentaires", "🎥"),
        new("podcasts", "Podcasts luisteren", "🎧"),
        new("radio-hits", "Radiohits", "📻"),
        new("live-concerts", "Live concerten", "🎶"),
        new("music-festivals", "Muziekfestivals", "🎉"),
        new("dance-parties", "Dansfeestjes", "🪩"),
        new("quiet-evenings", "Rustige avonden thuis", "🛋️"),
        new("candlelight-dinners", "Diner bij kaarslicht", "🕯️"),
        new("sunset-watching", "Zonsondergangen", "🌅"),
        new("rainy-days", "Regenachtige dagen", "🌧️"),
        new("snowy-days", "Sneeuwdagen", "❄️"),
        new("amusement-parks", "Pretparken", "🎢"),
    };

    private static readonly IReadOnlyDictionary<string, PreferenceOption> _preferenceOptionsById =
        _preferenceOptions.ToDictionary(option => option.Id, option => option, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, string> _preferenceIdByName =
        _preferenceOptions.ToDictionary(option => option.Name, option => option.Id, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyDictionary<string, int> _preferenceOrderById =
        _preferenceOptions
            .Select((option, index) => new { option.Id, index })
            .ToDictionary(entry => entry.Id, entry => entry.index, StringComparer.OrdinalIgnoreCase);

    private static readonly IReadOnlyList<PreferenceOption> _chatLineOptions = new List<PreferenceOption>
    {
        new("greeting-how-are-you", "Hoi! Hoe gaat het met je vandaag?"),
        new("weekend-plans", "Heb je leuke plannen voor dit weekend?"),
        new("weather-check", "Wat vind je van het weer vandaag?"),
        new("favorite-hobby", "Wat doe je het liefst in je vrije tijd?"),
        new("share-highlight", "Wat was het leukste dat je deze week meemaakte?"),
        new("coffee-invite", "Zin om binnenkort samen koffie te drinken?"),
        new("movie-talk", "Heb je onlangs nog een leuke film gezien?"),
        new("music-question", "Welke muziek luister je graag?"),
        new("book-recommendation", "Heb je nog een boekentip voor mij?"),
        new("food-question", "Wat eet jij het liefst als comfort food?"),
        new("walk-invite", "Zullen we binnenkort eens een wandeling maken?"),
        new("fun-fact", "Ik wil graag een leuk weetje horen over jou!"),
        new("gratitude", "Waar ben jij vandaag dankbaar voor?"),
        new("motivation", "Wat geeft jou energie op een drukke dag?"),
        new("relax-tip", "Hoe ontspan jij het liefst na een lange dag?"),
        new("game-question", "Speel je graag spelletjes?"),
        new("sport-chat", "Welke sport kijk of doe jij het liefst?"),
        new("travel-dream", "Welke plek wil je ooit nog bezoeken?"),
        new("memory-share", "Vertel eens over een mooie herinnering."),
        new("goal-question", "Waar kijk je deze maand het meest naar uit?"),
        new("support-offer", "Laat het me weten als ik iets voor je kan doen!"),
        new("photo-share", "Ik ben benieuwd naar je laatste foto, wil je die delen?"),
        new("daily-check-in", "Wat houdt je vandaag bezig?"),
        new("morning-message", "Goedemorgen! Heb je lekker geslapen?"),
        new("evening-message", "Slaap zacht straks, wat ga je nog doen vanavond?"),
        new("compliment", "Ik waardeer het echt om met jou te praten!"),
        new("laugh-question", "Waar heb je laatst hard om gelachen?"),
        new("learning", "Wat wil je graag nog leren?"),
        new("pet-talk", "Heb je huisdieren? Vertel eens!"),
        new("recipe-share", "Heb je een favoriet recept dat ik moet proberen?"),
    };

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

    private ProfileModel _model = new();
    private ProfileDraft _draft;
    private UserDto.CurrentUser? _currentUser;

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

    private bool _isToastVisible;
    private string _toastMessage = string.Empty;
    private CancellationTokenSource? _toastCts;

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
    [Inject] private UserContextService UserContext { get; set; } = default!;

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
    private string CurrentName => _isEditing ? _draft.Name : _model.Name;

    protected override async Task OnInitializedAsync()
    {
        try
        {
            var currentUser = await UserContext.InitializeAsync();
            if (currentUser is null)
            {
                _loadError = "Je bent niet aangemeld. Log opnieuw in om je profiel te bekijken.";
                return;
            }

            var memberSince = FormatMemberSince(currentUser.CreatedAt);
            _currentUser = currentUser;
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

    private static string FormatMemberSince(DateTime createdAt)
    {
        if (createdAt == default)
        {
            return "Actief sinds onbekend";
        }

        var culture = CultureInfo.GetCultureInfo("nl-BE");
        var formatted = createdAt.ToString("MMMM yyyy", culture);
        formatted = culture.TextInfo.ToTitleCase(formatted);
        return $"Actief sinds {formatted}";
    }

    private Task NavigateBack()
    {
        NavigationManager.NavigateTo("/");
        return Task.CompletedTask;
    }

    private void BeginEdit()
    {
        if (_isLoading || HasError)
        {
            return;
        }

        _draft = ProfileDraft.FromModel(_model);
        _pickerSelection = _selectedHobbyIds.ToHashSet();
        _initialHobbyIds = _selectedHobbyIds.ToHashSet();
        _initialLikeIds = _selectedLikeIds.ToList();
        _initialDislikeIds = _selectedDislikeIds.ToList();
        _preferencePickerSelection = new HashSet<string>(_selectedLikeIds, StringComparer.OrdinalIgnoreCase);
        _preferencePickerMode = PreferencePickerMode.None;
        _isPreferencePickerOpen = false;
        _preferencePickerSearch = string.Empty;
        _initialChatLineIds = _selectedChatLineIds.ToList();
        _chatLinePickerSelection = new HashSet<string>(_selectedChatLineIds, StringComparer.OrdinalIgnoreCase);
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        _isEditing = true;
    }

    private void CancelEdit()
    {
        _draft = ProfileDraft.FromModel(_model);
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
        _selectedChatLineIds = OrderChatLineIds(_initialChatLineIds);
        _chatLinePickerSelection = new HashSet<string>(_selectedChatLineIds, StringComparer.OrdinalIgnoreCase);
        _isChatLinePickerOpen = false;
        _chatLinePickerSearch = string.Empty;
        _newChatLineText = string.Empty;
        _newChatLineError = string.Empty;
        _isAddingCustomChatLine = false;
        _model = _model with { DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds) };
        UpdateInterestsModel();

        _isEditing = false;
    }

    private async Task SaveEdit()
    {
        if (_isLoading || HasError || _isSaving)
        {
            return;
        }

        _isSaving = true;

        var request = new UserRequest.UpdateCurrentUser
        {
            Name = _draft.Name?.Trim() ?? string.Empty,
            Email = _draft.Email?.Trim() ?? string.Empty,
            Biography = _draft.Bio ?? string.Empty,
            Gender = _draft.Gender,
            AvatarUrl = string.IsNullOrWhiteSpace(_draft.AvatarUrl) ? ProfileModel.DefaultAvatar : _draft.AvatarUrl,
            HobbyIds = _selectedHobbyIds.ToList(),
            LikePreferenceIds = _selectedLikeIds.ToList(),
            DislikePreferenceIds = _selectedDislikeIds.ToList(),
            DefaultChatLines = BuildChatLineTexts(_selectedChatLineIds)
        };

        try
        {
            var result = await UserContext.UpdateCurrentUserAsync(request);
            if (result is { IsSuccess: true, Value.User: not null })
            {
                var updatedUser = result.Value.User;
                _currentUser = updatedUser;
                var memberSince = FormatMemberSince(updatedUser.CreatedAt);
                _model = ProfileModel.FromUser(updatedUser, memberSince);
                _draft = ProfileDraft.FromModel(_model);
                SyncSelectionFromModel();
                _isEditing = false;
                await ShowToastAsync("Wijzigingen toegepast");
                return;
            }

            var errorMessage = ExtractErrorMessage(result);
            await ShowToastAsync(errorMessage);
        }
        catch
        {
            await ShowToastAsync("Er ging iets mis bij het opslaan van je profiel.");
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
                _model = _model with { AvatarUrl = dataUrl };
            }
        }
        catch
        {
            // Ignore failures and keep existing avatar.
        }
    }

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
        await ShowToastAsync("Hobby's bijgewerkt");
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
        _preferencePickerMode = PreferencePickerMode.None;
        _preferencePickerSearch = string.Empty;

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

        await ShowToastAsync("Favoriete chatzinnen bijgewerkt");
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

        var censored = WordFilter.Censor(input).Trim();

        if (string.IsNullOrWhiteSpace(censored))
        {
            _newChatLineError = "Deze zin kan niet worden gebruikt.";
            return Task.CompletedTask;
        }

        var id = ResolveChatLineId(censored);

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

        await ShowToastAsync(message);
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

    private List<string> OrderPreferenceIds(IEnumerable<string> ids)
    {
        return ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => _preferenceOrderById.TryGetValue(id, out var order) ? order : int.MaxValue)
            .ThenBy(id => GetPreferenceName(id), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string ResolvePreferenceId(string value)
    {
        var normalized = NormalizePreferenceValue(value);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return string.Empty;
        }

        if (_preferenceOptionsById.TryGetValue(normalized, out var optionById))
        {
            return optionById.Id;
        }

        if (_preferenceIdByName.TryGetValue(normalized, out var optionId))
        {
            return optionId;
        }

        if (!_customPreferenceOptions.ContainsKey(normalized))
        {
            _customPreferenceOptions[normalized] = normalized;
        }

        return normalized;
    }

    private static string NormalizePreferenceValue(string value)
        => value.Trim();

    private string GetPreferenceName(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        if (_preferenceOptionsById.TryGetValue(id, out var option))
        {
            return option.Name;
        }

        if (_customPreferenceOptions.TryGetValue(id, out var custom))
        {
            return custom;
        }

        return id;
    }

    private string GetPreferenceLabel(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        if (_preferenceOptionsById.TryGetValue(id, out var option))
        {
            return option.Label;
        }

        if (_customPreferenceOptions.TryGetValue(id, out var custom))
        {
            return custom;
        }

        return id;
    }

    private IReadOnlyList<PreferenceChip> BuildPreferenceChips(IEnumerable<string> ids)
    {
        var chips = new List<PreferenceChip>();
        foreach (var id in ids)
        {
            var label = GetPreferenceLabel(id);
            if (!string.IsNullOrWhiteSpace(label))
            {
                chips.Add(new PreferenceChip(id, label));
            }
        }

        return chips;
    }

    private List<string> OrderChatLineIds(IEnumerable<string> ids)
    {
        return ids
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Select(id => id.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(id => _chatLineOrderById.TryGetValue(id, out var order) ? order : int.MaxValue)
            .ThenBy(id => GetChatLineText(id), StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private string ResolveChatLineId(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalized = value.Trim();

        if (_chatLineOptionsById.TryGetValue(normalized, out var optionById))
        {
            return optionById.Id;
        }

        if (_chatLineIdByName.TryGetValue(normalized, out var optionId))
        {
            return optionId;
        }

        if (!_customChatLineOptions.ContainsKey(normalized))
        {
            _customChatLineOptions[normalized] = normalized;
        }

        return normalized;
    }

    private string GetChatLineText(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return string.Empty;
        }

        if (_chatLineOptionsById.TryGetValue(id, out var option))
        {
            return option.Name;
        }

        if (_customChatLineOptions.TryGetValue(id, out var custom))
        {
            return custom;
        }

        if (_chatLineIdByName.TryGetValue(id, out var resolvedId) && _chatLineOptionsById.TryGetValue(resolvedId, out var resolvedOption))
        {
            return resolvedOption.Name;
        }

        return id;
    }

    private IReadOnlyList<PreferenceChip> BuildChatLineChips(IEnumerable<string> ids)
    {
        var chips = new List<PreferenceChip>();
        foreach (var id in ids)
        {
            var text = GetChatLineText(id);
            if (!string.IsNullOrWhiteSpace(text))
            {
                chips.Add(new PreferenceChip(id, text));
            }
        }

        return chips;
    }

    private IReadOnlyList<string> BuildChatLineTexts(IEnumerable<string> ids)
    {
        var result = new List<string>();
        foreach (var id in ids)
        {
            var text = GetChatLineText(id);
            if (!string.IsNullOrWhiteSpace(text))
            {
                result.Add(text);
            }
        }

        return result;
    }

    private static bool RemovePreference(List<string> list, string id)
    {
        var index = list.FindIndex(existing => string.Equals(existing, id, StringComparison.OrdinalIgnoreCase));
        if (index >= 0)
        {
            list.RemoveAt(index);
            return true;
        }

        return false;
    }

    private void UpdateInterestsModel()
    {
        var interests = new List<ProfileInterestModel>();

        foreach (var likeId in _selectedLikeIds)
        {
            var label = GetPreferenceName(likeId);
            if (!string.IsNullOrWhiteSpace(label))
            {
                interests.Add(new ProfileInterestModel("Like", label, null));
            }
        }

        foreach (var dislikeId in _selectedDislikeIds)
        {
            var label = GetPreferenceName(dislikeId);
            if (!string.IsNullOrWhiteSpace(label))
            {
                interests.Add(new ProfileInterestModel("Dislike", null, label));
            }
        }

        _model = _model with { Interests = interests };
    }

    private static ProfileHobbyModel? CreateHobbyModel(string id)
    {
        var option = _hobbyOptions.FirstOrDefault(o => string.Equals(o.Id, id, StringComparison.Ordinal));
        return option is null ? null : new ProfileHobbyModel(option.Id, option.Name, option.Emoji);
    }

    private static string ExtractErrorMessage(Result<UserResponse.CurrentUser>? result)
    {
        if (result is null)
        {
            return "Er ging iets mis bij het opslaan van je profiel.";
        }

        if (result.Status == ResultStatus.Unauthorized)
        {
            return "Je bent niet aangemeld. Log opnieuw in om je profiel te bewerken.";
        }

        if (result.Status == ResultStatus.Invalid && result.ValidationErrors?.Count > 0)
        {
            var validationMessages = result.ValidationErrors
                .Select(error => error.ErrorMessage)
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToList();

            if (validationMessages.Count > 0)
            {
                return string.Join(" ", validationMessages);
            }
        }

        if (result.Errors?.Count > 0)
        {
            var errorMessages = result.Errors
                .Where(message => !string.IsNullOrWhiteSpace(message))
                .ToList();

            if (errorMessages.Count > 0)
            {
                return string.Join(" ", errorMessages);
            }
        }

        return "Er ging iets mis bij het opslaan van je profiel.";
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
