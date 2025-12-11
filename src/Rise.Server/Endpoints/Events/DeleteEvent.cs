using Rise.Shared.Events;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Events;

public class DeleteEvent(IEventService eventService)
    :Endpoint<EventRequest.DeleteEventRequest, Result<EventResponse.DeleteEventResponse>>
{
    public override void Configure()
    {
        Delete("/api/events/{EventId}");
        Roles(AppRoles.Supervisor, AppRoles.Administrator);
    }
    
    public override Task<Result<EventResponse.DeleteEventResponse>> ExecuteAsync(EventRequest.DeleteEventRequest req, CancellationToken ct)
    {
        return eventService.DeleteEvent(req, ct);
    }
}