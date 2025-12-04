using Ardalis.Result;
using Microsoft.AspNetCore.WebUtilities;
using Rise.Shared.Common;
using Rise.Shared.Emergencies;
using System.Net.Http.Json;

namespace Rise.Client.Emergencies;

public class EmergencyService(HttpClient httpClient) : IEmergencyService
{
    public async Task<Result<EmergencyResponse.Create>> CreateEmergencyAsync(EmergencyRequest.CreateEmergency request, CancellationToken ctx = default)
    {
        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync("/api/emergencies", request, ctx);
        }
        catch (HttpRequestException)
        {
            return Result<EmergencyResponse.Create>.Error("Kon geen verbinding maken met de server voor noodmeldingen.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ctx);
            return Result<EmergencyResponse.Create>.Error(error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<EmergencyResponse.Create>>(cancellationToken: ctx);
        return result ?? Result<EmergencyResponse.Create>.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<EmergencyResponse.GetEmergencies>> GetEmergenciesAsync(QueryRequest.SkipTake request, CancellationToken ctx = default)
    {
        try
        {
            var queryParameters = new Dictionary<string, string?>
            {
                ["Skip"] = request.Skip.ToString(),
                ["Take"] = request.Take.ToString(),
                ["SearchTerm"] = request.SearchTerm ?? string.Empty,
                ["OrderBy"] = request.OrderBy,
                ["OrderDescending"] = request.OrderDescending.ToString(),
            };

            var url = QueryHelpers.AddQueryString("/api/emergencies", queryParameters);
            var result = await httpClient.GetFromJsonAsync<Result<EmergencyResponse.GetEmergencies>>(url, cancellationToken: ctx);

            return result ?? Result<EmergencyResponse.GetEmergencies>.Error("Kon noodmeldingen niet laden.");
        }
        catch (HttpRequestException)
        {
            return Result<EmergencyResponse.GetEmergencies>.Error("Kon geen verbinding maken met de server voor noodmeldingen.");
        }
    }

    public async Task<Result<EmergencyResponse.GetEmergency>> GetEmergencyAsync(int id, CancellationToken ctx = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<Result<EmergencyResponse.GetEmergency>>($"/api/emergencies/{id}", cancellationToken: ctx);

            return result ?? Result<EmergencyResponse.GetEmergency>.Error("Kon noodmelding niet laden.");
        }
        catch (HttpRequestException)
        {
            return Result<EmergencyResponse.GetEmergency>.Error("Kon geen verbinding maken met de server voor noodmeldingen.");
        }
    }
}
