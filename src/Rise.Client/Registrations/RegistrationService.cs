using System.Collections.Generic;
using System.Net.Http.Json;
using Rise.Shared.Registrations;

namespace Rise.Client.Registrations;

public class RegistrationService(HttpClient httpClient) : IRegistrationService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<List<RegistrationResponse.ListItem>>> GetRequestsAsync(CancellationToken ct = default)
    {
        var result = await _httpClient
            .GetFromJsonAsync<Result<List<RegistrationResponse.ListItem>>>("/api/registrations/requests", ct);

        return result ?? Result<List<RegistrationResponse.ListItem>>.Error("Kon de registraties niet ophalen.");
    }

    public async Task<Result> ApproveAsync(int requestId, int supervisorId, CancellationToken ct = default)
    {
        var body = new RegistrationRequest.Approve(supervisorId);
        var response = await _httpClient.PostAsJsonAsync($"/api/registrations/{requestId}/approve", body, ct);

        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: ct);

        if (result is null)
        {
            return Result.Error("Onverwacht antwoord van de server.");
        }

        return result;
    }

    public async Task<Result> RejectAsync(int requestId, int supervisorId, CancellationToken ct = default)
    {
        var body = new RegistrationRequest.Reject(supervisorId);
        var response = await _httpClient.PostAsJsonAsync($"/api/registrations/{requestId}/reject", body, ct);

        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: ct);

        if (result is null)
        {
            return Result.Error("Onverwacht antwoord van de server.");
        }

        return result;
    }
}
