using Ardalis.Result;
using Rise.Shared.Events;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Events;

public class AddEvent(IEventService eventService)
    : Endpoint<EventRequest.AddEventRequest, Result<EventResponse.AddEventResponse>>
{
    public override void Configure()
    {
        Post("/api/events");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }

    public override Task<Result<EventResponse.AddEventResponse>> ExecuteAsync(EventRequest.AddEventRequest req, CancellationToken ct)
    {
        return eventService.AddEvent(req, ct);
    }
}
