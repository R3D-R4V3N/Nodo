using System.Collections.Generic;
using Rise.Shared.Organizations;

namespace Rise.Client.Organizations;

public interface IOrganizationService
{
    Task<Result<List<OrganizationResponse.ListItem>>> GetOrganizationsAsync(
        CancellationToken cancellationToken = default);
}
