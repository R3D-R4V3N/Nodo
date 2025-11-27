using Rise.Client.Offline;
using Rise.Shared.Users;
using System.Net.Http.Json;

namespace Rise.Client.Users;

public class UserService(HttpClient httpClient, OfflineQueueService offlineQueueService) : IUserService
{
    private readonly HttpClient _http = httpClient;
    private readonly OfflineQueueService _offlineQueueService = offlineQueueService;
    public async Task<Result<UserResponse.CurrentUser>> UpdateUserAsync(string accountId, UserRequest.UpdateCurrentUser request, CancellationToken ctx = default)
    {
        if (!await _offlineQueueService.IsOnlineAsync())
        {
            await _offlineQueueService.QueueOperationAsync(_http.BaseAddress?.ToString() ?? string.Empty,
                $"/api/users/{accountId}", HttpMethod.Put, request, cancellationToken: ctx);
            return Result<UserResponse.CurrentUser>.Error("Geen netwerkverbinding: de update is offline opgeslagen en wordt verstuurd zodra er verbinding is.");
        }

        HttpResponseMessage response;
        try
        {
            response = await _http.PutAsJsonAsync($"/api/users/{accountId}", request, ctx);
        }
        catch (HttpRequestException)
        {
            await _offlineQueueService.QueueOperationAsync(_http.BaseAddress?.ToString() ?? string.Empty,
                $"/api/users/{accountId}", HttpMethod.Put, request, cancellationToken: ctx);
            return Result<UserResponse.CurrentUser>.Error("Kon geen verbinding maken: de update is opgeslagen en wordt verzonden zodra de verbinding terug is.");
        }

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            return Result.Error(error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<UserResponse.CurrentUser>>(cancellationToken: ctx);

        if (result is null)
        {
            return Result.Error("Kon het serverantwoord niet verwerken.");
        }

        return result;
    }
    public async Task<Result<UserResponse.CurrentUser>> GetUserAsync(string accountId, CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _http.GetFromJsonAsync<Result<UserResponse.CurrentUser>>(
                $"api/users/{accountId}", cancellationToken);

            // gebruik de correcte generieke Error-methode
            return result ?? Result<UserResponse.CurrentUser>.Error("Kon de gebruikersinformatie niet laden.");
        }
        catch (HttpRequestException)
        {
            return Result<UserResponse.CurrentUser>.Error("Offline: gebruikersgegevens kunnen niet vernieuwd worden zonder verbinding.");
        }
    }
}