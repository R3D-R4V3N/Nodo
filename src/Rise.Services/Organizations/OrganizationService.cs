using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Shared.Organizations;

namespace Rise.Services.Organizations;

public class OrganizationService(ApplicationDbContext dbContext) : IOrganizationService
{
    public async Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken ct = default)
    {
        var organizations = await dbContext.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto.Summary
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
            })
            .ToListAsync(ct);

        return Result.Success(new OrganizationResponse.List
        {
            Organizations = organizations,
        });
    }
}
