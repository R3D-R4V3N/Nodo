using System.Linq;
using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Shared.Organizations;

namespace Rise.Services.Organizations;

public class OrganizationService(ApplicationDbContext dbContext) : IOrganizationService
{
    public async Task<Result<OrganizationResponse.GetOrganizations>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var organizations = await dbContext.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto.Summary
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new OrganizationResponse.GetOrganizations
        {
            Organizations = organizations
        });
    }
}
