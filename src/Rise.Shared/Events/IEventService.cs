namespace Rise.Shared.Events;

public interface IEventService
{
    Task<Result<EventsResponse.GetEvents>> GetEventsAsync(CancellationToken ctx = default);
    Task<Result<EventResponse.ToggleInterest>> ToggleInterestAsync(int eventId, CancellationToken ctx = default);
}