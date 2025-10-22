using System.Net.Http.Json;
using Rise.Shared.Profile;

namespace Rise.Client.Profile;

public class ProfileApiClient(HttpClient httpClient) : IProfileService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<ProfileResponse.Envelope>> GetAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var result = await _httpClient
                .GetFromJsonAsync<Result<ProfileResponse.Envelope>>("/api/profile", cancellationToken: cancellationToken);

            return result ?? Result.Error("Kon het profiel niet laden.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fout bij het ophalen van het profiel");
            return Result.Error("Kon het profiel niet laden.");
        }
    }

    public async Task<Result<ProfileResponse.Envelope>> UpdateAsync(ProfileRequest.UpdateProfile request, CancellationToken cancellationToken = default)
    {
        try
        {
            using var response = await _httpClient.PutAsJsonAsync("/api/profile", request, cancellationToken);
            var result = await response.Content.ReadFromJsonAsync<Result<ProfileResponse.Envelope>>(cancellationToken: cancellationToken);

            if (result is not null)
            {
                return result;
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result.Error($"Opslaan mislukt ({(int)response.StatusCode}).");
            }

            return Result.Error("Onbekend antwoord ontvangen.");
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Fout bij het bijwerken van het profiel");
            return Result.Error("Opslaan mislukt.");
        }
    }
}
