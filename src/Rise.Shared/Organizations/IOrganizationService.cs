using Ardalis.Result;

namespace Rise.Shared.Organizations;

public interface IOrganizationService
{
    Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken ct = default);
}
