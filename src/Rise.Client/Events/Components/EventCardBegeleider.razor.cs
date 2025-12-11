using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Components;
using Rise.Shared.Events;

namespace Rise.Client.Events.Components;

public partial class EventCardBegeleider
{
    [Parameter] public int EventId { get; set; }
    [Parameter] public string ImageUrl { get; set; } = "";
    [Parameter] public string Title { get; set; } = "";
    [Parameter] public string Location { get; set; } = "";
    [Parameter] public DateTime Date { get; set; } = DateTime.Now;
    [Parameter] public IEnumerable<EventDto.InterestedUser> InterestedUsers { get; set; } = Array.Empty<EventDto.InterestedUser>();
    [Parameter] public string? Description { get; set; }
    [Parameter] public double Price { get; set; }
    [Parameter] public EventCallback<int> OnDelete { get; set; }

    [Inject] private IEventService EventService { get; set; } = default!;

    private int InterestedCount => InterestedUsers?.Count() ?? 0;

    private string FormatDateDutch(DateTime date)
    {
        return date.ToString("d MMM 'om' HH:mm", new CultureInfo("nl-BE"));
    }
    
    

    private async Task HandleDeleteClick()
    {
        var request = new EventRequest.DeleteEventRequest
        {
            EventId = EventId
        };

        var result = await EventService.DeleteEvent(request);

        if (result.IsSuccess && OnDelete.HasDelegate)
            await OnDelete.InvokeAsync(EventId);
    }
}
