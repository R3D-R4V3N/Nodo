using Microsoft.AspNetCore.Components;
using Rise.Shared.Events;

namespace Rise.Client.Events.Pages;

public partial class BegeleiderDashboard
{
    [Inject] public required IEventService EventService { get; set; }
    
    private IEnumerable<EventDto.Get>? _events;
    private string? _errorMessage;
    private bool _showAddModal;
    private bool _isSaving;
    private string? _addEventError;
    private EventRequest.AddEventRequest _newEvent = CreateDefaultRequest();

    protected override async Task OnInitializedAsync()
    {
        await LoadEventsAsync();
    }

    private async Task LoadEventsAsync()
    {
        var result = await EventService.GetEventsAsync();

        if (result.IsSuccess && result.Value is not null)
            _events = result.Value.Events;
        else
            _errorMessage = string.Join(", ", result.Errors);
    }
    
    private async Task HandleEventDeleted(int eventId)
    {
        if (_events != null)
        {
            _events = _events.Where(e => e.Id != eventId).ToList();
            StateHasChanged();
        }
    }

    private void OpenAddModal()
    {
        _newEvent = CreateDefaultRequest();
        _addEventError = null;
        _showAddModal = true;
    }

    private void CloseModal()
    {
        _showAddModal = false;
        _addEventError = null;
    }

    private async Task HandleAddEvent()
    {
        if (_isSaving)
            return;

        if (string.IsNullOrWhiteSpace(_newEvent.Name) ||
            string.IsNullOrWhiteSpace(_newEvent.Location) ||
            _newEvent.Date == default)
        {
            _addEventError = "Vul alle verplichte velden in.";
            return;
        }

        if (_newEvent.Price < 0)
        {
            _addEventError = "De prijs kan niet negatief zijn.";
            return;
        }

        _isSaving = true;
        _addEventError = null;

        var addRequest = new EventRequest.AddEventRequest
        {
            Name = _newEvent.Name.Trim(),
            Date = _newEvent.Date,
            Location = _newEvent.Location.Trim(),
            Price = _newEvent.Price,
            ImageUrl = _newEvent.ImageUrl?.Trim() ?? string.Empty
        };

        var result = await EventService.AddEvent(addRequest);

        if (result.IsSuccess)
        {
            await LoadEventsAsync();
            CloseModal();
        }
        else
        {
            _addEventError = string.Join(", ", result.Errors);
        }

        _isSaving = false;
    }

    private static EventRequest.AddEventRequest CreateDefaultRequest() => new()
    {
        Name = string.Empty,
        Location = string.Empty,
        ImageUrl = string.Empty,
        Price = 0,
        Date = DateTime.Now.AddDays(1)
    };
}
