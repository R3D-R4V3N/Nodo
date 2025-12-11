using Rise.Shared.Events;

namespace Rise.Shared.Events;

public interface IEventService
{
    Task<Result<EventResponse.GetEvents>> GetEventsAsync(CancellationToken ctx = default);
    Task<Result<EventResponse.AddEventResponse>> AddEvent(EventRequest.AddEventRequest request, CancellationToken ctx = default);
    Task<Result<EventResponse.ToggleInterest>> ToggleInterestAsync(int eventId, CancellationToken ctx = default);
    Task<Result<EventResponse.DeleteEventResponse>> DeleteEvent(EventRequest.DeleteEventRequest req, CancellationToken ct = default);
}
