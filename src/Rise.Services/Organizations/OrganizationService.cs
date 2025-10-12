using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Shared.Organizations;

namespace Rise.Services.Organizations;

public class OrganizationService(ApplicationDbContext dbContext) : IOrganizationService
{
    public async Task<Result<OrganizationResponse.List>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var organizations = await dbContext.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto.Index
            {
                Id = o.Id,
                Name = o.Name
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new OrganizationResponse.List
        {
            Organizations = organizations
        });
    }
}
