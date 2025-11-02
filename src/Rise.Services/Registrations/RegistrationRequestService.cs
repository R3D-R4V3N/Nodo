using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Registrations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

namespace Rise.Services.Registrations;

public class RegistrationRequestService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider,
    UserManager<IdentityUser> userManager) : IRegistrationRequestService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public async Task<Result<List<RegistrationRequestResponse.PendingItem>>> GetPendingAsync(CancellationToken ct = default)
    {
        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var currentSupervisor = await _dbContext.Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == accountId, ct);

        var isAdmin = _sessionContextProvider.User?.IsInRole(AppRoles.Administrator) ?? false;

        if (currentSupervisor is null && !isAdmin)
        {
            return Result.Forbidden();
        }

        var query = _dbContext.RegistrationRequests
            .Include(r => r.Organization)
            .Where(r => r.Status == RegistrationRequestStatus.Pending);

        if (currentSupervisor is not null)
        {
            query = query.Where(r => r.OrganizationId == currentSupervisor.Organization.Id);
        }

        var pendingRequests = await query
            .AsNoTracking()
            .ToListAsync(ct);

        if (pendingRequests.Count == 0)
        {
            return Result.Success(new List<RegistrationRequestResponse.PendingItem>());
        }

        var organizationIds = pendingRequests
            .Select(r => r.OrganizationId)
            .Distinct()
            .ToArray();

        var supervisors = await _dbContext.Supervisors
            .Where(s => organizationIds.Contains(EF.Property<int>(s, "OrganizationId")))
            .Select(s => new
            {
                s.Id,
                Name = $"{s.FirstName.Value} {s.LastName.Value}",
                OrganizationId = EF.Property<int>(s, "OrganizationId")
            })
            .ToListAsync(ct);

        var supervisorLookup = supervisors
            .GroupBy(s => s.OrganizationId)
            .ToDictionary(g => g.Key, g => g
                .Select(s => new RegistrationRequestResponse.SupervisorListItem(s.Id, s.Name))
                .ToList() as IReadOnlyList<RegistrationRequestResponse.SupervisorListItem>);

        var accountIds = pendingRequests
            .Select(r => r.AccountId)
            .Distinct()
            .ToArray();

        var accounts = await _userManager.Users
            .Where(u => accountIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email })
            .ToListAsync(ct);

        var emailLookup = accounts.ToDictionary(a => a.Id, a => a.Email ?? string.Empty);

        var result = pendingRequests
            .Select(r => new RegistrationRequestResponse.PendingItem
            {
                Id = r.Id,
                FirstName = r.FirstName.Value,
                LastName = r.LastName.Value,
                Email = emailLookup.TryGetValue(r.AccountId, out var email) ? email : string.Empty,
                OrganizationId = r.OrganizationId,
                OrganizationName = r.Organization.Name.Value,
                RequestedAt = r.CreatedAt,
                Supervisors = supervisorLookup.TryGetValue(r.OrganizationId, out var supervisorsForOrg)
                    ? supervisorsForOrg
                    : Array.Empty<RegistrationRequestResponse.SupervisorListItem>()
            })
            .ToList();

        return Result.Success(result);
    }

    public async Task<Result> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default)
    {
        if (request is null || request.SupervisorId <= 0)
        {
            return Result.Invalid(new ValidationError(nameof(request.SupervisorId), "Kies een supervisor."));
        }

        var accountId = _sessionContextProvider.User?.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var registrationRequest = await _dbContext.RegistrationRequests
            .Include(r => r.Organization)
            .SingleOrDefaultAsync(r => r.Id == requestId, ct);

        if (registrationRequest is null)
        {
            return Result.NotFound("Registratieaanvraag niet gevonden.");
        }

        if (registrationRequest.Status != RegistrationRequestStatus.Pending)
        {
            return Result.Conflict("Deze registratieaanvraag werd al verwerkt.");
        }

        var currentSupervisor = await _dbContext.Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == accountId, ct);

        var isAdmin = _sessionContextProvider.User?.IsInRole(AppRoles.Administrator) ?? false;

        if (currentSupervisor is not null
            && currentSupervisor.Organization.Id != registrationRequest.OrganizationId)
        {
            return Result.Forbidden("Je kan enkel aanvragen van je eigen organisatie goedkeuren.");
        }

        if (currentSupervisor is null && !isAdmin)
        {
            return Result.Forbidden();
        }

        var supervisor = await _dbContext.Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.Id == request.SupervisorId, ct);

        if (supervisor is null)
        {
            return Result.NotFound("Supervisor niet gevonden.");
        }

        if (supervisor.Organization.Id != registrationRequest.OrganizationId)
        {
            return Result.Invalid(new ValidationError(nameof(request.SupervisorId), "Kies een supervisor van dezelfde organisatie."));
        }

        var identityUser = await _userManager.FindByIdAsync(registrationRequest.AccountId);

        if (identityUser is null)
        {
            return Result.NotFound("Account niet gevonden.");
        }

        if (await _dbContext.Users.AnyAsync(u => u.AccountId == registrationRequest.AccountId, ct))
        {
            return Result.Conflict("Er bestaat al een profiel voor dit account.");
        }

        var biographyResult = Biography.Create("Nog geen biografie beschikbaar.");
        if (!biographyResult.IsSuccess)
        {
            return Result.CriticalError("Kon geen standaard biografie instellen.");
        }

        var avatarUrl = $"https://ui-avatars.com/api/?name={Uri.EscapeDataString($"{registrationRequest.FirstName.Value} {registrationRequest.LastName.Value}")}&background=0B6532&color=ffffff&size=200";
        var avatarResult = AvatarUrl.Create(avatarUrl);
        if (!avatarResult.IsSuccess)
        {
            return Result.CriticalError("Kon geen standaard avatar instellen.");
        }

        var fontSizeResult = FontSize.Create(12);
        if (!fontSizeResult.IsSuccess)
        {
            return Result.CriticalError("Kon gebruikersinstellingen niet initialiseren.");
        }

        var user = new User()
        {
            AccountId = registrationRequest.AccountId,
            FirstName = registrationRequest.FirstName,
            LastName = registrationRequest.LastName,
            Biography = biographyResult.Value,
            AvatarUrl = avatarResult.Value,
            BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Organization = registrationRequest.Organization,
            UserSettings = new UserSetting()
            {
                FontSize = fontSizeResult.Value,
                IsDarkMode = false,
            }
        };

        user.AssignSupervisor(supervisor);

        registrationRequest.Approve(supervisor);

        _dbContext.Users.Add(user);

        await _userManager.AddToRoleAsync(identityUser, AppRoles.User);
        await _dbContext.SaveChangesAsync(ct);

        return Result.Success();
    }
}
