using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Rise.Shared.Registrations;

namespace Rise.Client.Registrations;

public class RegistrationRequestClient(HttpClient httpClient) : IRegistrationRequestClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<List<RegistrationRequestResponse.PendingItem>>> GetPendingAsync(CancellationToken ct = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<List<RegistrationRequestResponse.PendingItem>>>(
            "/api/registration-requests",
            cancellationToken: ct);

        return result ?? Result<List<RegistrationRequestResponse.PendingItem>>.Error("Kon de registratieaanvragen niet laden.");
    }

    public async Task<Result> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default)
    {
        var response = await _httpClient.PostAsJsonAsync(
            $"/api/registration-requests/{requestId}/approve",
            request,
            ct);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return Result.Error($"Kon registratieaanvraag niet goedkeuren: {errorContent}");
        }

        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: ct);
        return result ?? Result.Error("Onbekende fout bij het goedkeuren van de registratieaanvraag.");
    }
}
