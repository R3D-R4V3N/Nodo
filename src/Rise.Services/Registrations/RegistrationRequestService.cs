using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users.Registrations;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Rise.Services.Registrations;

public class RegistrationRequestService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IRegistrationRequestService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    public async Task<Result<IReadOnlyCollection<RegistrationRequestDto.ListItem>>> GetPendingRequestsForSupervisorAsync(
        CancellationToken cancellationToken = default)
    {
        var supervisorAccountId = _sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(supervisorAccountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await _dbContext.Supervisors
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.AccountId == supervisorAccountId, cancellationToken);

        if (supervisor is null)
        {
            return Result.Unauthorized("Je hebt geen toegang tot deze aanvragen.");
        }

        var requests = await _dbContext.UserRegistrationRequests
            .AsNoTracking()
            .Where(r => r.OrganizationId == supervisor.OrganizationId && r.Status == RegistrationStatus.Pending)
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .OrderBy(r => r.CreatedAt)
            .Select(r => new RegistrationRequestDto.ListItem
            {
                Id = r.Id,
                AccountId = r.AccountId,
                Email = r.Email,
                FullName = r.FullName,
                OrganizationId = r.OrganizationId,
                OrganizationName = r.Organization.Name,
                AssignedSupervisorId = r.AssignedSupervisorId,
                AssignedSupervisorName = r.AssignedSupervisor != null
                    ? $"{r.AssignedSupervisor.FirstName} {r.AssignedSupervisor.LastName}"
                    : null,
                Status = r.Status.ToDto(),
                CreatedAt = r.CreatedAt,
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IReadOnlyCollection<RegistrationRequestDto.ListItem>>(requests);
    }
}
