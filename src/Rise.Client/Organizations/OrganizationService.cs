using Ardalis.Result;
using Rise.Client.Offline;
using Rise.Shared.Organizations;
using System.Net.Http.Json;

namespace Rise.Client.Organizations;

public class OrganizationService(HttpClient httpClient, CacheStoreService cacheStoreService) : IOrganizationService
{
    private readonly CacheStoreService _cacheStoreService = cacheStoreService;

    public async Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken ct = default)
    {
        try
        {
            var result = await httpClient.GetFromJsonAsync<Result<OrganizationResponse.List>>("/api/organizations", cancellationToken: ct);

            if (result is { IsSuccess: true, Value: { } organizationResponse })
            {
                await _cacheStoreService.UpsertOrganizationsAsync(organizationResponse.Organizations, ct);
            }

            return result ?? Result<OrganizationResponse.List>.Error("Kon de organisaties niet laden.");
        }
        catch (HttpRequestException)
        {
            var cached = await _cacheStoreService.GetOrganizationsAsync(ct);
            if (cached.Any())
            {
                return Result.Success(new OrganizationResponse.List { Organizations = cached }).MarkCached();
            }

            return Result<OrganizationResponse.List>.Error("Kon de organisaties niet laden.");
        }
    }
}
