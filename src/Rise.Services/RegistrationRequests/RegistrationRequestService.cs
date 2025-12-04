using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Registrations;
using Rise.Domain.Users;
using Rise.Domain.Users.Settings;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Services.RegistrationRequests.Mapper;
using Rise.Shared.Assets;
using Rise.Shared.Identity;
using Rise.Shared.RegistrationRequests;
using Rise.Shared.Identity.Accounts;
using Rise.Services.Users.Mapper;

namespace Rise.Services.RegistrationRequests;

public class RegistrationRequestService(
    ApplicationDbContext dbContext,
    UserManager<IdentityUser> userManager,
    ISessionContextProvider sessionContextProvider,
    ILogger<RegistrationRequestService> logger,
    IPasswordHasher<IdentityUser> passwordHasher) : IRegistrationRequestService
{
    public async Task<Result> CreateAsync(AccountRequest.Register request, CancellationToken ctx = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result.Invalid(new ValidationError(nameof(request.Email), "Ongeldige gegevens."));
        }

        var normalizedEmail = userManager.NormalizeEmail(request.Email);

        if (string.IsNullOrWhiteSpace(normalizedEmail))
        {
            return Result.Invalid(new ValidationError(nameof(request.Email), "Ongeldig e-mailadres."));
        }

        if (await userManager.FindByEmailAsync(normalizedEmail) is not null)
        {
            return Result.Conflict("Er bestaat al een account met dit e-mailadres.");
        }

        var hasPendingRequest = await dbContext
            .RegistrationRequests
            .AnyAsync(r => r.Email.Value == Email.Create(request.Email).Value.Value
                && r.Status.StatusType == RegistrationStatusType.Pending, ctx);

        if (hasPendingRequest)
        {
            return Result.Conflict("Er is al een lopende registratieaanvraag voor dit e-mailadres.");
        }

        var organization = await dbContext
            .Organizations
            .SingleOrDefaultAsync(o => o.Id == request.OrganizationId, ctx);

        if (organization is null)
        {
            return Result.Invalid(new ValidationError(nameof(request.OrganizationId), "Ongeldige organisatie geselecteerd."));
        }

        var identityUser = new IdentityUser 
        {
            UserName = request.Email, 
            Email = request.Email 
        };
        var hashedPassword = passwordHasher.HashPassword(identityUser, request.Password);

        var registration = RegistrationRequest.Create(
            request.Email,
            request.FirstName!,
            request.LastName!,
            request.BirthDate ?? DateOnly.FromDateTime(DateTime.UtcNow),
            request.Gender.ToDomain(),
            request.AvatarDataUrl!,
            hashedPassword,
            organization
        );

        registration.AvatarUrl ??= AvatarUrl.Create(DefaultImages.GetProfile(request.Email));

        dbContext.RegistrationRequests.Add(registration);

        await dbContext.SaveChangesAsync(ctx);

        return Result.SuccessWithMessage("Uw aanvraag is ingediend en wacht op goedkeuring door een begeleider.");

    }

    public async Task<Result<RegistrationRequestResponse.PendingList>> GetPendingAsync(CancellationToken ct = default)
    {
        var principal = sessionContextProvider.User;

        if (principal is null)
        {
            return Result.Unauthorized();
        }

        var accountId = principal.GetUserId();

        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await dbContext.Supervisors
            .AsNoTracking()
            .SingleOrDefaultAsync(s => s.AccountId == accountId, ct);

        var isAdministrator = principal.IsInRole(AppRoles.Administrator);

        if (supervisor is null && !isAdministrator)
        {
            return Result.Unauthorized();
        }

        var supervisorsQuery = dbContext
            .Supervisors
            .AsNoTracking();

        var pendingQuery = dbContext
            .RegistrationRequests
            .AsNoTracking()
            .Where(r => r.Status.StatusType == RegistrationStatusType.Pending);

        if (supervisor is not null)
        {
            supervisorsQuery = supervisorsQuery
                .Where(s => s.Organization.Id == supervisor.Organization.Id);

            pendingQuery = pendingQuery
                .Where(r => r.Organization.Id == supervisor.Organization.Id);
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
                    .TryGetValue(x.Organization.Id, out var options)
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
        var principal = sessionContextProvider.User;

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

        var approver = await dbContext
            .Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == approverAccountId, ct);

        if (approver is null && !isAdministrator)
        {
            return Result.Unauthorized();
        }

        var registration = await dbContext
            .RegistrationRequests
            .Include(r => r.Organization)
            .SingleOrDefaultAsync(r => r.Id == requestId, ct);

        if (registration is null)
        {
            return Result.NotFound();
        }

        if (approver is not null && registration.Organization.Id != approver.Organization.Id)
        {
            return Result.Unauthorized();
        }

        if (registration.Status is not { StatusType: RegistrationStatusType.Pending })
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        var assignedSupervisor = await dbContext
            .Supervisors
            .SingleOrDefaultAsync(s => s.Id == request.AssignedSupervisorId, ct);

        if (assignedSupervisor is null)
        {
            return Result.Invalid(new ValidationError(nameof(request.AssignedSupervisorId), "Begeleider werd niet gevonden."));
        }

        if (assignedSupervisor.Organization.Id != registration.Organization.Id)
        {
            return Result.Invalid(new ValidationError(nameof(request.AssignedSupervisorId), "Begeleider behoort niet tot dezelfde organisatie."));
        }

        var approvalResult = registration.Approve(approver ?? assignedSupervisor, assignedSupervisor);
        if (!approvalResult.IsSuccess)
        {
            return Result.Conflict(approvalResult.Errors.ToArray());
        }

        if (await userManager.FindByEmailAsync(registration.Email) is not null)
        {
            return Result.Error("Er bestaat al een account met dit e-mailadres.");
        }

        var normalizedEmail = userManager.NormalizeEmail(registration.Email);

        var identityUser = new IdentityUser
        {
            UserName = registration.Email,
            Email = registration.Email,
            EmailConfirmed = true,
            PasswordHash = registration.PasswordHash,
            NormalizedEmail = normalizedEmail,
            NormalizedUserName = normalizedEmail,
        };

        using var trans = await dbContext.Database.BeginTransactionAsync(ct);

        var createResult = await userManager.CreateAsync(identityUser);

        if (!createResult.Succeeded)
        {
            logger.LogError("Kon IdentityUser niet aanmaken voor registratie {RegistrationId}: {Errors}", requestId, string.Join(',', createResult.Errors.Select(e => e.Description)));
            return Result.Error("Kon het account niet aanmaken.");
        }


        await userManager.AddToRoleAsync(identityUser, AppRoles.User);

        var newUser = new User
        {
            AccountId = identityUser.Id,
            FirstName = FirstName.Create(registration.FirstName),
            LastName = LastName.Create(registration.LastName),
            Biography = Biography.Create("Nieuw bij Nodo."),
            AvatarUrl = AvatarUrl.Create(string.IsNullOrWhiteSpace(registration.AvatarUrl)
                ? DefaultImages.GetProfile(identityUser.Email)
                : registration.AvatarUrl),
            BirthDay = BirthDay.Create(registration.BirthDay),
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

        dbContext.Users.Add(newUser);

        await dbContext.SaveChangesAsync(ct);

        await trans.CommitAsync(ct);

        return Result.Success(new RegistrationRequestResponse.Approve
        {
            RegistrationRequestId = registration.Id,
        });
    }
}
