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
        var response = await _httpClient.PostAsJsonAsync("api/emergencies", request, ctx);
        return await ReadResultAsync<EmergencyResponse.Create>(response, ctx, "Kon geen noodmelding aanmaken.");
    }

    public async Task<Result<EmergencyResponse.GetEmergencies>> GetEmergenciesAsync(
        QueryRequest.SkipTake request,
        CancellationToken ctx = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<EmergencyResponse.GetEmergencies>>(
            $"api/emergencies?skip={request.Skip}&take={request.Take}&searchTerm={request.SearchTerm}",
            ctx);

        return result ?? Result<EmergencyResponse.GetEmergencies>.Error("Kon de noodmeldingen niet laden.");
    }

    public async Task<Result<EmergencyResponse.GetEmergency>> GetEmergencyAsync(int id, CancellationToken ctx = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<EmergencyResponse.GetEmergency>>(
            $"api/emergencies/{id}",
            ctx);

        return result ?? Result<EmergencyResponse.GetEmergency>.Error("Kon de noodmelding niet laden.");
    }

    public async Task<Result<EmergencyResponse.UpdateStatus>> UpdateStatusAsync(
        EmergencyRequest.UpdateStatus request,
        CancellationToken ctx = default)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/emergencies/{request.EmergencyId}/status",
            request,
            ctx);

        return await ReadResultAsync<EmergencyResponse.UpdateStatus>(
            response,
            ctx,
            "Kon de status van de noodmelding niet bijwerken.");
    }

    private static async Task<Result<T>> ReadResultAsync<T>(HttpResponseMessage response, CancellationToken ctx, string fallbackMessage)
    {
        try
        {
            var result = await response.Content.ReadFromJsonAsync<Result<T>>(cancellationToken: ctx);
            return result ?? Result<T>.Error(fallbackMessage);
        }
        catch
        {
            return Result<T>.Error(fallbackMessage);
        }
    }
}
