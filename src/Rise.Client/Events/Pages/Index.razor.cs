using System.Globalization;
using Microsoft.AspNetCore.Components;
using Rise.Client.State;
using Rise.Shared.Events;

namespace Rise.Client.Events.Pages;

public partial class Index
{
    [Inject] public required IEventService EventService { get; set; }
    [Inject] public required UserState UserState { get; set; }
    
    private IEnumerable<EventDto.Get>? _events;
    private string? _errorMessage;

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

    private async Task HandleToggleInterest(int eventId)
    {
        var result = await EventService.ToggleInterestAsync(eventId);

        if (result.IsSuccess)
        {
            // Reload events to get updated data
            await LoadEventsAsync();
            StateHasChanged();
        }
        else
        {
            _errorMessage = string.Join(", ", result.Errors);
        }
    }

    private bool IsUserInterested(EventDto.Get eventItem)
    {
        if (UserState.User?.Id == null) return false;
        return eventItem.InterestedUsers.Any(u => u.Id == UserState.User.Id);
    }

    private string FormatDateDutch(DateTime date)
    {
        var culture = new CultureInfo("nl-NL");
        return date.ToString("ddd d MMM 'om' HH:mm", culture);
    }
}