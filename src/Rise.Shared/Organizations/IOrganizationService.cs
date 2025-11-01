namespace Rise.Shared.Organizations;

public interface IOrganizationService
{
    Task<Result<OrganizationResponse.GetOrganizations>> GetOrganizationsAsync(CancellationToken cancellationToken = default);
}
