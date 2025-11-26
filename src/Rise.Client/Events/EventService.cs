using System.Net.Http.Json;
using Rise.Shared.Events;
using Rise.Shared.Events.GetEvents;

namespace Rise.Client.Events;

public class EventService(HttpClient httpClient): IEventService
{
    public async Task<Result<GetEventsResponse.GetEvents>> GetEventsAsync(CancellationToken ctx = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<GetEventsResponse.GetEvents>>("api/events", ctx);
        return result!;
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
}