using System.Collections.Generic;
using System.Globalization;
using System.IO;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Rise.Client.Profile.Models;
using Rise.Client.Users;

namespace Rise.Client.Profile.Components;

public partial class ProfileScreen : ComponentBase, IDisposable
{
    private ProfileModel _model = ProfileModel.CreateDefault();
    private ProfileDraft _draft;

    private bool _isEditing;
    private bool _isLoading = true;
    private string? _loadError;

    private bool _isToastVisible;
    private string _toastMessage = string.Empty;
    private CancellationTokenSource? _toastCts;

    public ProfileScreen()
    {
        _draft = ProfileDraft.FromModel(_model);
    }

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private UserContextService UserContext { get; set; } = default!;

    private bool IsEditing => _isEditing;
    private bool IsLoading => _isLoading;
    private bool HasError => !string.IsNullOrWhiteSpace(_loadError);
    private string? ErrorMessage => _loadError;
    private IReadOnlyList<ProfileInterestModel> Interests => _model.Interests;
    private IReadOnlyList<ProfileHobbyModel> Hobbies => _model.Hobbies;
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
        _isEditing = true;
    }

    private void CancelEdit()
    {
        _draft = ProfileDraft.FromModel(_model);
        _isEditing = false;
    }

    private async Task SaveEdit()
    {
        _model = _draft.ApplyTo(_model);
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
