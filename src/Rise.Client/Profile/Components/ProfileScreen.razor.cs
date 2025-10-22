using System.Collections.Immutable;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Rise.Client.Profile.Models;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen : ComponentBase, IDisposable
{
    private static readonly IReadOnlyList<InterestOption> _interestOptions = new List<InterestOption>
    {
        new("zwemmen",     "Zwemmen",     "🏊"),
        new("voetbal",     "Voetbal",     "⚽"),
        new("rugby",       "Rugby",       "🏉"),
        new("basketbal",   "Basketbal",   "🏀"),
        new("gaming",      "Gaming",      "🎮"),
        new("koken",       "Koken",       "🍳"),
        new("bakken",      "Bakken",      "🧁"),
        new("wandelen",    "Wandelen",    "🚶"),
        new("fietsen",     "Fietsen",     "🚴"),
        new("tekenen",     "Tekenen",     "✏️"),
        new("schilderen",  "Schilderen",  "🎨"),
        new("muziek",      "Muziek",      "🎵"),
        new("zingen",      "Zingen",      "🎤"),
        new("dansen",      "Dansen",      "🕺"),
        new("lezen",       "Lezen",       "📚"),
        new("tuinieren",   "Tuinieren",   "🌱"),
        new("vissen",      "Vissen",      "🎣"),
        new("kamperen",    "Kamperen",    "🎕️"),
        new("reizen",      "Reizen",      "✈️"),
        new("fotografie",  "Fotografie",  "📸"),
        new("film",        "Film",        "🎬"),
        new("series",      "Series",      "📺"),
        new("dieren",      "Dieren",      "🐶"),
        new("yoga",        "Yoga",        "🧘‍♂️"),
        new("fitness",     "Fitness",     "🏋️‍♂️"),
        new("hardlopen",   "Hardlopen",   "🏃‍♂️"),
        new("kaarten",     "Kaarten",     "🃏"),
        new("puzzelen",    "Puzzelen",    "🧩"),
        new("bordspellen", "Bordspellen", "🎲"),
        new("knutselen",   "Knutselen",   "✂️")
    }.ToImmutableList();

    private const int MaxInterests = 3;

    private ProfileModel _model = ProfileModel.CreateDefault();
    private ProfileDraft _draft;

    private readonly HashSet<string> _selectedInterestIds = new();
    private HashSet<string> _pickerSelection = new();

    private bool _isEditing;
    private bool _isPickerOpen;
    private string _pickerSearch = string.Empty;

    private bool _isToastVisible;
    private string _toastMessage = string.Empty;
    private CancellationTokenSource? _toastCts;

    public ProfileScreen()
    {
        _draft = ProfileDraft.FromModel(_model);
    }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private bool IsEditing => _isEditing;
    private bool IsPickerOpen => _isPickerOpen;
    private IReadOnlyList<InterestOption> InterestOptions => _interestOptions;
    private IReadOnlyList<InterestOption> SelectedInterests => InterestOptions.Where(i => _selectedInterestIds.Contains(i.Id)).ToList();
    private string DisplayName => string.IsNullOrWhiteSpace(CurrentName) ? "Jouw Naam" : CurrentName;
    private string CurrentName => _isEditing ? _draft.Name : _model.Name;

    private Task NavigateBack()
    {
        NavigationManager.NavigateTo("/");
        return Task.CompletedTask;
    }

    private void BeginEdit()
    {
        _draft = ProfileDraft.FromModel(_model);
        _isEditing = true;
    }

    private void CancelEdit()
    {
        _draft = ProfileDraft.FromModel(_model);
        _isEditing = false;
    }

    private async Task SaveEdit()
    {
        _model = _draft.ToModel();
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

    private void OpenInterestsPicker()
    {
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
        await ShowToastAsync("Interesses bijgewerkt");
    }

    private Task RemoveInterest(string id)
    {
        _selectedInterestIds.Remove(id);
        return Task.CompletedTask;
    }

    private Task UpdatePickerSearch(string value)
    {
        _pickerSearch = value;
        return Task.CompletedTask;
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
