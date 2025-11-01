using System.Collections.Generic;
using System.Net.Http.Json;
using Rise.Shared.Organizations;

namespace Rise.Client.Organizations;

public class OrganizationService(HttpClient httpClient) : IOrganizationService
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<Result<List<OrganizationResponse.ListItem>>> GetOrganizationsAsync(
        CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetFromJsonAsync<Result<List<OrganizationResponse.ListItem>>>(
            "/api/organizations",
            cancellationToken);

        return result ?? Result<List<OrganizationResponse.ListItem>>.Error("Kon de organisaties niet laden.");
    }
}
