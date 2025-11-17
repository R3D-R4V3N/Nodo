using System;
using System.Linq;
using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rise.Domain.Registrations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.RegistrationRequests.Mapper;
using Rise.Shared.Assets;
using Rise.Shared.Identity;
using Rise.Shared.RegistrationRequests;
using Rise.Shared.Users;

namespace Rise.Services.RegistrationRequests;

public class RegistrationRequestService(
    ApplicationDbContext dbContext,
    UserManager<IdentityUser> userManager,
    ISessionContextProvider sessionContextProvider,
    ILogger<RegistrationRequestService> logger) : IRegistrationRequestService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly UserManager<IdentityUser> _userManager = userManager;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    private readonly ILogger<RegistrationRequestService> _logger = logger;

    public async Task<Result<RegistrationRequestResponse.PendingList>> GetPendingAsync(CancellationToken ct = default)
    {
        var principal = _sessionContextProvider.User;

        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();

        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await _dbContext.Supervisors
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.AccountId == accountId, ct);

        var isAdministrator = principal.IsInRole(AppRoles.Administrator);

        if (supervisor is null && !isAdministrator)
        {
            return Result.Unauthorized();
        }

        var supervisorsQuery = _dbContext
            .Supervisors
            .AsNoTracking();

        var pendingQuery = _dbContext
            .RegistrationRequests
            .AsNoTracking()
            .Where(r => r.Status == RegistrationStatus.Pending);

        if (supervisor is not null)
        {
            supervisorsQuery = supervisorsQuery
                .Where(s => s.Organization.Id == supervisor.Organization.Id);

            pendingQuery = pendingQuery
                .Where(r => r.OrganizationId == supervisor.Organization.Id);
        }

        var supervisorLookup = await supervisorsQuery
            .OrderBy(s => s.FirstName.Value)
            .ThenBy(s => s.LastName.Value)
            .Include(s => s.Organization)
            .GroupBy(s => s.Organization.Id)
            .ToDictionaryAsync(
                g => g.Key,
                g => g.Select(RegistrationMapper.ToSupervisorOption).ToList(), 
            ct);

        var pendingRows = await pendingQuery
            .Include(r => r.Organization)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(ct);

        var pendingRequests = pendingRows
            .Select(x => 
            {
                var supervisorLst = supervisorLookup
                    .TryGetValue(x.OrganizationId, out var options)
                        ? options : [];

                return RegistrationMapper.ToPending(x, supervisorLst);
            }).ToList();

        return Result.Success(
            new RegistrationRequestResponse.PendingList
            {
                Requests = pendingRequests,
            }
        );
    }

    public async Task<Result<RegistrationRequestResponse.Approve>> ApproveAsync(int requestId, RegistrationRequestRequest.Approve request, CancellationToken ct = default)
    {
        var principal = _sessionContextProvider.User;

        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var approverAccountId = principal.GetUserId();
        var isAdministrator = principal.IsInRole(AppRoles.Administrator);

        if (string.IsNullOrWhiteSpace(approverAccountId) && !isAdministrator)
        {
            return Result.Unauthorized();
        }

        var approver = await _dbContext
            .Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == approverAccountId, ct);

        if (approver is null && !isAdministrator)
        {
            return Result.Unauthorized();
        }

        var registration = await _dbContext
            .RegistrationRequests
            .Include(r => r.Organization)
            .SingleOrDefaultAsync(r => r.Id == requestId, ct);

        if (registration is null)
        {
            return Result.NotFound();
        }

        if (approver is not null && registration.OrganizationId != approver.Organization.Id)
        {
            return Result.Unauthorized();
        }

        if (registration.Status != RegistrationStatus.Pending)
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        var assignedSupervisor = await _dbContext
            .Supervisors
            .SingleOrDefaultAsync(s => s.Id == request.AssignedSupervisorId, ct);

        if (assignedSupervisor is null)
        {
            return Result.Invalid(new ValidationError(nameof(request.AssignedSupervisorId), "Begeleider werd niet gevonden."));
        }

        if (assignedSupervisor.Organization.Id != registration.OrganizationId)
        {
            return Result.Invalid(new ValidationError(nameof(request.AssignedSupervisorId), "Begeleider behoort niet tot dezelfde organisatie."));
        }

        var approvalResult = registration.Approve(approver ?? assignedSupervisor, assignedSupervisor);
        if (!approvalResult.IsSuccess)
        {
            return Result.Conflict(approvalResult.Errors.ToArray());
        }

        if (await _userManager.FindByEmailAsync(registration.Email) is not null)
        {
            return Result.Error("Er bestaat al een account met dit e-mailadres.");
        }

        var identityUser = new IdentityUser
        {
            UserName = registration.Email,
            Email = registration.Email,
            EmailConfirmed = true,
            PasswordHash = registration.PasswordHash,
            NormalizedEmail = registration.NormalizedEmail,
            NormalizedUserName = registration.NormalizedEmail,
        };

        var createResult = await _userManager.CreateAsync(identityUser);

        if (!createResult.Succeeded)
        {
            _logger.LogError("Kon IdentityUser niet aanmaken voor registratie {RegistrationId}: {Errors}", requestId, string.Join(',', createResult.Errors.Select(e => e.Description)));
            return Result.Error("Kon het account niet aanmaken.");
        }

        await _userManager.AddToRoleAsync(identityUser, AppRoles.User);

        var newUser = new User
        {
            AccountId = identityUser.Id,
            FirstName = FirstName.Create(registration.FirstName),
            LastName = LastName.Create(registration.LastName),
            Biography = Biography.Create("Nieuw bij Nodo."),
            AvatarUrl = AvatarUrl.Create(string.IsNullOrWhiteSpace(registration.AvatarUrl)
                ? DefaultImages.GetProfile(identityUser.Email)
                : registration.AvatarUrl),
            BirthDay = registration.BirthDate,
            Gender = registration.Gender,
            UserSettings = new UserSetting
            {
                FontSize = FontSize.Create(12),
                IsDarkMode = false,
            },
            Organization = registration.Organization,
        };

        var chatLineResults = new[]
        {
            newUser.UserSettings.AddChatTextLine("Hoi! Ik ben net toegekomen."),
            newUser.UserSettings.AddChatTextLine("Fijn om hier te zijn!"),
        };

        if (chatLineResults.Any(r => !r.IsSuccess))
        {
            var errors = chatLineResults.SelectMany(r => r.Errors).ToArray();
            var errorMessage = string.Join(" ", errors);
            return Result.Error(errorMessage);
        }

        _dbContext.Users.Add(newUser);

        await _dbContext.SaveChangesAsync(ct);

        return Result.Success(new RegistrationRequestResponse.Approve
        {
            RegistrationRequestId = registration.Id,
        });
    }

}
