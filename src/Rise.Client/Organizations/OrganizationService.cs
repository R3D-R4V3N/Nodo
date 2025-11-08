using Ardalis.Result;
using Rise.Shared.Organizations;
using System.Net.Http.Json;

namespace Rise.Client.Organizations;

public class OrganizationService(HttpClient httpClient) : IOrganizationService
{
    public async Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken ct = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<OrganizationResponse.List>>("/api/organizations", cancellationToken: ct);

        return result ?? Result<OrganizationResponse.List>.Error("Kon de organisaties niet laden.");
    }
}
