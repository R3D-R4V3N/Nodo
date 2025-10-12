using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Rise.Shared.Organizations;

namespace Rise.Client.Organizations;

public class OrganizationService(HttpClient httpClient) : IOrganizationService
{
    public async Task<Result<OrganizationResponse.List>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await httpClient.GetFromJsonAsync<Result<OrganizationResponse.List>>("api/organizations", cancellationToken);
        return result ?? Result.Error("Kon de organisaties niet laden.");
    }
}
