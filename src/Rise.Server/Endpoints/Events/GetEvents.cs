using Rise.Shared.Events;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Events;

public class GetEvents(IEventService eventService) : EndpointWithoutRequest<Result<EventResponse.GetEvents>>
{
    public override void Configure()
    {
        Get("/api/events");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
        Summary(s =>
        {
            s.Summary = "List events";
            s.Description = "Retrieves all events available to the authenticated user.";
        });
    }

    public override Task<Result<EventResponse.GetEvents>> ExecuteAsync(CancellationToken ct)
    {
        return eventService.GetEventsAsync(ct);
    }
}
