using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Rise.Client.Profile.Models;
using Rise.Client.Users;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen : ComponentBase, IDisposable
{
    private static readonly IReadOnlyList<HobbyOption> _hobbyOptions = new List<HobbyOption>
    {
        new("Swimming", "Zwemmen", "🏊"),
        new("Football", "Voetbal", "⚽"),
        new("Rugby", "Rugby", "🏉"),
        new("Basketball", "Basketbal", "🏀"),
        new("Gaming", "Gaming", "🎮"),
        new("Cooking", "Koken", "🍳"),
        new("Baking", "Bakken", "🧁"),
        new("Hiking", "Wandelen", "🚶"),
        new("Cycling", "Fietsen", "🚴"),
        new("Drawing", "Tekenen", "✏️"),
        new("Painting", "Schilderen", "🎨"),
        new("Music", "Muziek", "🎵"),
        new("Singing", "Zingen", "🎤"),
        new("Dancing", "Dansen", "🕺"),
        new("Reading", "Lezen", "📚"),
        new("Gardening", "Tuinieren", "🌱"),
        new("Fishing", "Vissen", "🎣"),
        new("Camping", "Kamperen", "🎪"),
        new("Travel", "Reizen", "✈️"),
        new("Photography", "Fotografie", "📸"),
        new("Movies", "Films", "🎬"),
        new("Series", "Series", "📺"),
        new("Animals", "Dieren", "🐶"),
        new("Yoga", "Yoga", "🧘"),
        new("Fitness", "Fitness", "🏋️"),
        new("Running", "Hardlopen", "🏃"),
        new("Cards", "Kaarten", "🃏"),
        new("Puzzles", "Puzzelen", "🧩"),
        new("BoardGames", "Bordspellen", "🎲"),
        new("Crafts", "Knutselen", "✂️"),
    };

    private const int HobbySelectionLimit = 3;

    private ProfileModel _model = ProfileModel.CreateDefault();
    private ProfileDraft _draft;

    private readonly HashSet<string> _selectedHobbyIds = new();
    private HashSet<string> _initialHobbyIds = new();
    private HashSet<string> _pickerSelection = new();

    private bool _isEditing;
    private bool _isLoading = true;
    private string? _loadError;

    private bool _isPickerOpen;
    private string _pickerSearch = string.Empty;

    private bool _isToastVisible;
    private string _toastMessage = string.Empty;
    private CancellationTokenSource? _toastCts;

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
    private IReadOnlyList<ProfileInterestModel> Interests => _model.Interests;
    private IReadOnlyList<ProfileHobbyModel> Hobbies => _model.Hobbies;
    private IReadOnlyList<HobbyOption> HobbyOptions => _hobbyOptions;
    private IReadOnlyCollection<string> PickerSelection => _pickerSelection;
    private string PickerSearch => _pickerSearch;
    private bool IsPickerOpen => _isPickerOpen;
    private int MaxHobbies => HobbySelectionLimit;
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
        _isEditing = false;
    }

    private async Task SaveEdit()
    {
        _model = _draft.ApplyTo(_model);
        _initialHobbyIds = _selectedHobbyIds.ToHashSet();
        _isEditing = false;
        await ShowToastAsync("Wijzigingen toegepast");
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

    private Task UpdatePickerSearch(string value)
    {
        _pickerSearch = value;
        return Task.CompletedTask;
    }

    private static ProfileHobbyModel? CreateHobbyModel(string id)
    {
        var option = _hobbyOptions.FirstOrDefault(o => string.Equals(o.Id, id, StringComparison.Ordinal));
        return option is null ? null : new ProfileHobbyModel(option.Id, option.Name, option.Emoji);
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
