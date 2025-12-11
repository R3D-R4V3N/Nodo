using Rise.Shared.Events;

namespace Rise.Server.Endpoints.Events;

public class GetEvents(IEventService eventService) : EndpointWithoutRequest<Result<EventResponse.GetEvents>>
{
    public override void Configure()
    {
        Get("/api/events");
        AllowAnonymous();
    }

    public override Task<Result<EventResponse.GetEvents>> ExecuteAsync(CancellationToken ct)
    {
        return eventService.GetEventsAsync(ct);
    }
}
