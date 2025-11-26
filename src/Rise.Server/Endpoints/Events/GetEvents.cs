using Ardalis.Result;
using Rise.Shared.Events;
using Rise.Shared.Events.GetEvents;

namespace Rise.Server.Endpoints.Events;

public class GetEvents(IEventService eventService) : EndpointWithoutRequest<Result<GetEventsResponse.GetEvents>>
{
    public override void Configure()
    {
        Get("/api/events");
        AllowAnonymous();
    }

    public override Task<Result<GetEventsResponse.GetEvents>> ExecuteAsync(CancellationToken ct)
    {
        return eventService.GetEventsAsync(ct);
    }
}
