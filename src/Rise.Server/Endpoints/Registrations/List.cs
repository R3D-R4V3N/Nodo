using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Server.Endpoints.Registrations;

public class List(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider)
    : EndpointWithoutRequest<Result<List<RegistrationResponse.ListItem>>>
{
    public override void Configure()
    {
        Get("/api/registrations/requests");
        Roles(AppRoles.Administrator, AppRoles.Supervisor);
    }

    public override async Task<Result<List<RegistrationResponse.ListItem>>> ExecuteAsync(CancellationToken ct)
    {
        var user = sessionContextProvider.User;
        if (user is null)
        {
            return Result.Unauthorized();
        }

        var allowedOrganizationIds = new List<int>();

        if (user.IsInRole(AppRoles.Administrator))
        {
            allowedOrganizationIds = await dbContext.Organizations
                .Select(o => o.Id)
                .ToListAsync(ct);
        }
        else
        {
            var accountId = user.GetUserId();
            if (string.IsNullOrWhiteSpace(accountId))
            {
                return Result.Unauthorized();
            }

            var supervisorProfile = await dbContext.Supervisors
                .AsNoTracking()
                .Where(s => s.AccountId == accountId)
                .Select(s => s.Organization.Id)
                .FirstOrDefaultAsync(ct);

            if (supervisorProfile == 0)
            {
                return Result.Forbidden("Geen supervisorprofiel gevonden.");
            }

            allowedOrganizationIds.Add(supervisorProfile);
        }

        if (allowedOrganizationIds.Count == 0)
        {
            return Result.Success(new List<RegistrationResponse.ListItem>());
        }

        var registrations = await dbContext.RegistrationRequests
            .AsNoTracking()
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .Where(r => allowedOrganizationIds.Contains(r.OrganizationId))
            .OrderBy(r => r.Status)
            .ThenBy(r => r.LastName.Value)
            .ToListAsync(ct);

        if (registrations.Count == 0)
        {
            return Result.Success(new List<RegistrationResponse.ListItem>());
        }

        var accountIds = registrations
            .Select(r => r.AccountId)
            .Distinct()
            .ToList();

        var identityAccounts = await dbContext.Set<IdentityUser>()
            .Where(i => accountIds.Contains(i.Id))
            .Select(i => new { i.Id, i.Email })
            .ToListAsync(ct);

        var accountLookup = identityAccounts.ToDictionary(x => x.Id, x => x.Email ?? string.Empty);

        var supervisors = await dbContext.Supervisors
            .AsNoTracking()
            .Where(s => allowedOrganizationIds.Contains(s.Organization.Id))
            .Select(s => new
            {
                s.Id,
                Name = $"{s.FirstName.Value} {s.LastName.Value}",
                OrganizationId = s.Organization.Id
            })
            .ToListAsync(ct);

        var supervisorLookup = supervisors
            .GroupBy(s => s.OrganizationId)
            .ToDictionary(
                g => g.Key,
                g => g
                    .Select(s => new RegistrationResponse.SupervisorOption(s.Id, s.Name))
                    .ToList());

        var response = registrations
            .Select(r =>
            {
                var supervisorOptions = supervisorLookup.TryGetValue(r.OrganizationId, out var options)
                    ? (IReadOnlyList<RegistrationResponse.SupervisorOption>)options
                    : Array.Empty<RegistrationResponse.SupervisorOption>();

                return new RegistrationResponse.ListItem(
                    r.Id,
                    r.FirstName.Value,
                    r.LastName.Value,
                    accountLookup.TryGetValue(r.AccountId, out var email) ? email : string.Empty,
                    r.OrganizationId,
                    r.Organization.Name.Value,
                    r.Status.ToString(),
                    r.AssignedSupervisorId,
                    r.AssignedSupervisor is null
                        ? null
                        : $"{r.AssignedSupervisor.FirstName.Value} {r.AssignedSupervisor.LastName.Value}",
                    supervisorOptions);
            })
            .ToList();

        return Result.Success(response);
    }
}
