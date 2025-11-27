using Ardalis.Result;
using Rise.Client.Offline;
using Rise.Shared.Identity.Accounts;
using Rise.Shared.RegistrationRequests;
using System.Net.Http.Json;

namespace Rise.Client.RegistrationRequests;

public class RegistrationRequestService(HttpClient httpClient, OfflineQueueService offlineQueueService) : IRegistrationRequestService
{
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    public Task<Result> CreateAsync(AccountRequest.Register request, CancellationToken ct = default)
    {
        // dont implement, '/api/identity/accounts/register' makes use of the interface
        throw new NotImplementedException();
    }

    public async Task<Result<RegistrationRequestResponse.PendingList>> GetPendingAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<Result<RegistrationRequestResponse.PendingList>>("/api/registrations/pending", cancellationToken: ct);

            return result ?? Result<RegistrationRequestResponse.PendingList>.Error("Kon de aanvragen niet laden.");
        }
        catch (HttpRequestException)
        {
            return Result<RegistrationRequestResponse.PendingList>.Error("Offline: openstaande aanvragen kunnen niet vernieuwd worden zonder verbinding.");
        }
    }

    public async Task<Result<RegistrationRequestResponse.Approve>> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default)
    {
        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await _offlineQueueService.QueueOperationAsync(httpClient.BaseAddress?.ToString() ?? string.Empty,
                $"/api/registrations/{requestId}/approve", HttpMethod.Post, request, cancellationToken: ct);
            return Result<RegistrationRequestResponse.Approve>.Error("Geen netwerkverbinding: de goedkeuring is opgeslagen en wordt verstuurd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsJsonAsync($"/api/registrations/{requestId}/approve", request, ct);
        }
        catch (HttpRequestException)
        {
            await _offlineQueueService.QueueOperationAsync(httpClient.BaseAddress?.ToString() ?? string.Empty,
                $"/api/registrations/{requestId}/approve", HttpMethod.Post, request, cancellationToken: ct);
            return Result<RegistrationRequestResponse.Approve>.Error("Kon geen verbinding maken: de goedkeuring is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(ct);
            return Result<RegistrationRequestResponse.Approve>.Error(error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<RegistrationRequestResponse.Approve>>(cancellationToken: ct);

        return result ?? Result<RegistrationRequestResponse.Approve>.Error("Kon het serverantwoord niet verwerken.");
    }
}
