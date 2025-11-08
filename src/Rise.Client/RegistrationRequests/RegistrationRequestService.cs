using Ardalis.Result;
using Rise.Shared.RegistrationRequests;
using System.Net.Http.Json;

namespace Rise.Client.RegistrationRequests;

public class RegistrationRequestService(HttpClient httpClient) : IRegistrationRequestService
{
    public async Task<Result<RegistrationRequestResponse.PendingList>> GetPendingAsync(CancellationToken ct = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<RegistrationRequestResponse.PendingList>>("/api/registrations/pending", cancellationToken: ct);

        return result ?? Result<RegistrationRequestResponse.PendingList>.Error("Kon de aanvragen niet laden.");
    }

    public async Task<Result<RegistrationRequestResponse.Approve>> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default)
    {
        var response = await httpClient.PostAsJsonAsync($"/api/registrations/{requestId}/approve", request, ct);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<RegistrationRequestResponse.Approve>.Error(error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<RegistrationRequestResponse.Approve>>(cancellationToken: ct);

        return result ?? Result<RegistrationRequestResponse.Approve>.Error("Kon het serverantwoord niet verwerken.");
    }
}
