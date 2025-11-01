using System.Net.Http.Json;
using Rise.Shared.Registrations;

namespace Rise.Client.Supervisor.Registrations;

public class RegistrationService(HttpClient httpClient) : IRegistrationService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<RegistrationResponse.PendingList>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<RegistrationResponse.PendingList>>("/api/registrations/pending", cancellationToken);
        return result ?? Result<RegistrationResponse.PendingList>.Error("Kon openstaande aanvragen niet laden.");
    }

    public async Task<Result<RegistrationResponse.Detail>> GetDetailAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<RegistrationResponse.Detail>>($"/api/registrations/{registrationId}", cancellationToken);
        return result ?? Result<RegistrationResponse.Detail>.Error("Kon de aanvraagdetails niet ophalen.");
    }

    public async Task<Result> ApproveAsync(RegistrationRequest.Approve request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/registrations/{request.RegistrationId}/approve", request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: cancellationToken);
        return result ?? Result.Error("Kon de aanvraag niet goedkeuren.");
    }

    public async Task<Result> RejectAsync(RegistrationRequest.Reject request, CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.PostAsJsonAsync($"/api/registrations/{request.RegistrationId}/reject", request, cancellationToken);
        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: cancellationToken);
        return result ?? Result.Error("Kon de aanvraag niet afwijzen.");
    }
}
