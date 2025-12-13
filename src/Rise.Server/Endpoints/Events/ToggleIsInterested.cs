using Ardalis.Result;
using Rise.Shared.Events;
using Rise.Shared.Identity;

namespace Rise.Server.Endpoints.Events;

public class ToggleIsInterested(IEventService eventService) : EndpointWithoutRequest<Result<EventResponse.ToggleInterest>>
{
    public override void Configure()
    {
        Post("/api/events/{eventId:int}/interest");
        Roles(AppRoles.User, AppRoles.Supervisor, AppRoles.Administrator);
        Summary(s =>
        {
            s.Summary = "Toggle event interest";
            s.Description = "Marks the current user as interested/uninterested in the specified event.";
        });
    }

    public override Task<Result<EventResponse.ToggleInterest>> ExecuteAsync(CancellationToken ct)
    {
        var evenId = Route<int>("eventId");
        return eventService.ToggleInterestAsync(evenId, ct);
    }
}
