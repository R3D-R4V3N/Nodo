using Ardalis.Result;
using Rise.Domain.Organizations;
using Rise.Shared.Organizations;

namespace Rise.Services.Organizations;

public interface IOrganizationService
{
    Task<Result<IReadOnlyCollection<OrganizationDto.ListItem>>> GetOrganizationsAsync(CancellationToken cancellationToken = default);

    Task<Organization?> FindByIdAsync(int id, CancellationToken cancellationToken = default);
}
