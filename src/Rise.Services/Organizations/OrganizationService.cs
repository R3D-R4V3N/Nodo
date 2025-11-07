using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Organizations;
using Rise.Persistence;
using Rise.Shared.Organizations;
using System.Linq;

namespace Rise.Services.Organizations;

public class OrganizationService(ApplicationDbContext dbContext) : IOrganizationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;

    public async Task<Result<IReadOnlyCollection<OrganizationDto.ListItem>>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var organizations = await _dbContext.Organizations
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto.ListItem
            {
                Id = o.Id,
                Name = o.Name,
                Description = o.Description,
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyCollection<OrganizationDto.ListItem>>(organizations);
    }

    public Task<Organization?> FindByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.Organizations
            .SingleOrDefaultAsync(o => o.Id == id, cancellationToken);
    }
}
