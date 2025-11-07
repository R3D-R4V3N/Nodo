using System.Net.Http.Json;
using Rise.Shared.Organizations;
using Rise.Shared.Registrations;

namespace Rise.Client.Registrations;

public class RegistrationClient(HttpClient httpClient) : IRegistrationClient
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/organizations", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<OrganizationResponse.List>.Error(string.IsNullOrWhiteSpace(error)
                ? "Kon de lijst met organisaties niet laden."
                : error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<OrganizationResponse.List>>(cancellationToken: cancellationToken);
        return result ?? Result<OrganizationResponse.List>.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<RegistrationResponse.PendingList>> GetPendingRegistrationsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync("/api/organizations/registrations/pending", cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<RegistrationResponse.PendingList>.Error(string.IsNullOrWhiteSpace(error)
                ? "Kon de openstaande aanvragen niet laden."
                : error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<RegistrationResponse.PendingList>>(cancellationToken: cancellationToken);
        return result ?? Result<RegistrationResponse.PendingList>.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result<RegistrationResponse.Updated>> AssignSupervisorAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        var payload = new RegistrationRequest.AssignSupervisor { RegistrationId = registrationId };
        var response = await _httpClient.PostAsJsonAsync("/api/organizations/registrations/assign", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result<RegistrationResponse.Updated>.Error(string.IsNullOrWhiteSpace(error)
                ? "Kon de aanvraag niet koppelen."
                : error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result<RegistrationResponse.Updated>>(cancellationToken: cancellationToken);
        return result ?? Result<RegistrationResponse.Updated>.Error("Kon het serverantwoord niet verwerken.");
    }

    public async Task<Result> ApproveRegistrationAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        var payload = new RegistrationRequest.Approve { RegistrationId = registrationId };
        var response = await _httpClient.PostAsJsonAsync("/api/organizations/registrations/approve", payload, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            return Result.Error(string.IsNullOrWhiteSpace(error)
                ? "Kon de aanvraag niet goedkeuren."
                : error);
        }

        var result = await response.Content.ReadFromJsonAsync<Result>(cancellationToken: cancellationToken);
        return result ?? Result.Error("Kon het serverantwoord niet verwerken.");
    }
}
