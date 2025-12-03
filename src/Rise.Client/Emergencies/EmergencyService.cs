using System.Net.Http.Json;
using Ardalis.Result;
using Rise.Shared.Common;
using Rise.Shared.Emergencies;

namespace Rise.Client.Emergencies;

public class EmergencyService(HttpClient httpClient) : IEmergencyService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<EmergencyResponse.Create>> CreateEmergencyAsync(
        EmergencyRequest.CreateEmergency request,
        CancellationToken ctx = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await _httpClient.PostAsJsonAsync("api/emergencies", request, ctx);
        }
        catch (HttpRequestException)
        {
            return Result.Error("Kon geen verbinding maken om de noodmelding te versturen.");
        }

        var result = await response.Content.ReadFromJsonAsync<Result<EmergencyResponse.Create>>(cancellationToken: ctx);

        return result ?? Result.Error("Kon het antwoord van de server niet verwerken.");
    }

    public async Task<Result<EmergencyResponse.GetEmergencies>> GetEmergenciesAsync(
        QueryRequest.SkipTake request,
        CancellationToken ctx = default)
    {
        var query = $"api/emergencies?skip={request.Skip}&take={request.Take}" +
                    (string.IsNullOrWhiteSpace(request.SearchTerm)
                        ? string.Empty
                        : $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<EmergencyResponse.GetEmergencies>>(query, ctx);
            return result ?? Result.Error("Kon de noodmeldingen niet laden.");
        }
        catch (HttpRequestException)
        {
            return Result.Error("Kon geen verbinding maken om de noodmeldingen op te halen.");
        }
    }

    public async Task<Result<EmergencyResponse.GetEmergency>> GetEmergencyAsync(int id, CancellationToken ctx = default)
    {
        try
        {
            var result = await _httpClient.GetFromJsonAsync<Result<EmergencyResponse.GetEmergency>>(
                $"api/emergencies/{id}",
                ctx);
            return result ?? Result.Error("Kon de noodmelding niet laden.");
        }
        catch (HttpRequestException)
        {
            return Result.Error("Kon geen verbinding maken om de noodmelding op te halen.");
        }
    }
}
