using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;

namespace Rise.Shared.Organizations;

public interface IOrganizationService
{
    Task<Result<OrganizationResponse.List>> GetAllAsync(CancellationToken cancellationToken = default);
}
