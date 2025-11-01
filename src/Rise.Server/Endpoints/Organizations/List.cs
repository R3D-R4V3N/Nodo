using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Rise.Persistence;
using Rise.Shared.Organizations;

namespace Rise.Server.Endpoints.Organizations;

/// <summary>
/// Provides a list of available organizations.
/// </summary>
/// <param name="dbContext">Application database context.</param>
public class List(ApplicationDbContext dbContext)
    : EndpointWithoutRequest<Result<List<OrganizationResponse.ListItem>>>
{
    public override void Configure()
    {
        Get("/api/organizations");
        AllowAnonymous();
    }

    public override async Task<Result<List<OrganizationResponse.ListItem>>> ExecuteAsync(CancellationToken ct)
    {
        var organizations = await dbContext.Organizations
            .Include(o => o.Address)
                .ThenInclude(a => a.City)
            .Select(o => new OrganizationResponse.ListItem(
                o.Id,
                o.Name.Value,
                o.Address.ToString()))
            .ToListAsync(ct);

        return Result.Success(organizations);
    }
}
