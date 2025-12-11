using System.Net.Http.Json;
using Rise.Shared.Events;
namespace Rise.Client.Events;

public class EventService(HttpClient httpClient): IEventService
{
    public async Task<Result<EventResponse.GetEvents>> GetEventsAsync(CancellationToken ctx = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<EventResponse.GetEvents>>("api/events", ctx);
        return result!;
    }

    public async Task<Result<EventResponse.AddEventResponse>> AddEvent(EventRequest.AddEventRequest request, CancellationToken ctx = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("api/events", request, ctx);
        }
        catch (HttpRequestException e)
        {
            Console.WriteLine(e);
            throw;
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het aanmaken van evenement: {error}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<EventResponse.AddEventResponse>>(cancellationToken: ctx);
        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<EventResponse.ToggleInterest>> ToggleInterestAsync(int eventId, CancellationToken ctx = default)
    {
        var response = await httpClient.PostAsJsonAsync($"api/events/{eventId}/interest", new { }, ctx);
        
        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result.Error($"Fout bij het wijzigen van interesse: {error}");
        }
        
        var result = await response.Content.ReadFromJsonAsync<Result<EventResponse.ToggleInterest>>(cancellationToken: ctx);
        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<EventResponse.DeleteEventResponse>> DeleteEvent(EventRequest.DeleteEventRequest req, CancellationToken ct = default)
    {
        var response = await httpClient.DeleteAsync($"api/events/{req.EventId}", ct);
        
        if(!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result.Error($"Fout bij het verwijderen van evenement: {error}");
        }
        
        var result = await response.Content.ReadFromJsonAsync<Result<EventResponse.DeleteEventResponse>>(cancellationToken: ct);
        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }
}