using System.Net.Http.Json;
using Rise.Shared.Organizations;

namespace Rise.Client.Organizations;

public class OrganizationService(HttpClient httpClient) : IOrganizationService
{
    public async Task<Result<OrganizationResponse.GetOrganizations>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<OrganizationResponse.GetOrganizations>>("/api/organizations", cancellationToken);
        return result ?? Result.Error("Kon de organisaties niet laden.");
    }
}
