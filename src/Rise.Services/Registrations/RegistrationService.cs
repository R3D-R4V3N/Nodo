using Ardalis.Result;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;
using Rise.Domain.Users.Settings;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Identity;
using Rise.Shared.Organizations;
using Rise.Shared.Registrations;
using System.Linq;

namespace Rise.Services.Registrations;

public class RegistrationService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider,
    UserManager<IdentityUser> userManager) : IRegistrationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;
    private readonly UserManager<IdentityUser> _userManager = userManager;

    public async Task<Result<OrganizationResponse.List>> GetOrganizationsAsync(CancellationToken cancellationToken = default)
    {
        var organizations = await _dbContext.Organizations
            .AsNoTracking()
            .OrderBy(o => o.Name)
            .Select(o => new OrganizationDto.Summary
            {
                Id = o.Id,
                Name = o.Name,
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new OrganizationResponse.List
        {
            Organizations = organizations,
        });
    }

    public async Task<Result<RegistrationResponse.PendingList>> GetPendingRegistrationsAsync(CancellationToken cancellationToken = default)
    {
        var supervisorAccountId = _sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(supervisorAccountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await _dbContext.Supervisors
            .AsNoTracking()
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == supervisorAccountId, cancellationToken);

        if (supervisor is null)
        {
            return Result.Unauthorized();
        }

        var registrations = await _dbContext.UserRegistrations
            .AsNoTracking()
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .Where(r => r.OrganizationId == supervisor.OrganizationId && r.Status == RegistrationStatus.Pending)
            .OrderBy(r => r.RequestedAt)
            .ToListAsync(cancellationToken);

        var dtos = registrations
            .Select(RegistrationMapper.ToPendingDto)
            .ToList();

        return Result.Success(new RegistrationResponse.PendingList
        {
            Registrations = dtos,
        });
    }

    public async Task<Result<RegistrationResponse.Updated>> AssignSupervisorAsync(RegistrationRequest.AssignSupervisor request, CancellationToken cancellationToken = default)
    {
        var supervisorAccountId = _sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(supervisorAccountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await _dbContext.Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == supervisorAccountId, cancellationToken);

        if (supervisor is null)
        {
            return Result.Unauthorized();
        }

        var registration = await _dbContext.UserRegistrations
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .SingleOrDefaultAsync(r => r.Id == request.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return Result.NotFound();
        }

        if (registration.OrganizationId != supervisor.OrganizationId)
        {
            return Result.Unauthorized();
        }

        var assignResult = registration.AssignSupervisor(supervisor);
        if (!assignResult.IsSuccess)
        {
            return Result.Conflict(assignResult.Errors.ToArray());
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        await _dbContext.Entry(registration).Reference(r => r.AssignedSupervisor).LoadAsync(cancellationToken);

        var dto = RegistrationMapper.ToPendingDto(registration);
        return Result.Success(new RegistrationResponse.Updated
        {
            Registration = dto,
        });
    }

    public async Task<Result> ApproveRegistrationAsync(RegistrationRequest.Approve request, CancellationToken cancellationToken = default)
    {
        var supervisorAccountId = _sessionContextProvider.User?.GetUserId();

        if (string.IsNullOrWhiteSpace(supervisorAccountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await _dbContext.Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == supervisorAccountId, cancellationToken);

        if (supervisor is null)
        {
            return Result.Unauthorized();
        }

        var registration = await _dbContext.UserRegistrations
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .SingleOrDefaultAsync(r => r.Id == request.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return Result.NotFound();
        }

        if (registration.OrganizationId != supervisor.OrganizationId)
        {
            return Result.Unauthorized();
        }

        if (registration.AssignedSupervisorId != supervisor.Id)
        {
            return Result.Conflict("Alleen de toegewezen begeleider kan deze aanvraag goedkeuren.");
        }

        if (await _dbContext.Users.AnyAsync(u => u.AccountId == registration.AccountId, cancellationToken))
        {
            return Result.Conflict("Er bestaat al een profiel voor deze gebruiker.");
        }

        var identityUser = await _userManager.FindByIdAsync(registration.AccountId);
        if (identityUser is null)
        {
            return Result.NotFound("Het gekoppelde account bestaat niet meer.");
        }

        var approveResult = registration.Approve();
        if (!approveResult.IsSuccess)
        {
            return Result.Conflict(approveResult.Errors.ToArray());
        }

        var roleResult = await _userManager.AddToRoleAsync(identityUser, AppRoles.User);
        if (!roleResult.Succeeded)
        {
            return Result.Error(roleResult.Errors.First().Description);
        }

        var firstNameResult = FirstName.Create(registration.FirstName);
        if (!firstNameResult.IsSuccess)
        {
            return Result.Invalid(firstNameResult.Errors.Select(e => new ValidationError(nameof(registration.FirstName), e)).ToArray());
        }

        var lastNameResult = LastName.Create(registration.LastName);
        if (!lastNameResult.IsSuccess)
        {
            return Result.Invalid(lastNameResult.Errors.Select(e => new ValidationError(nameof(registration.LastName), e)).ToArray());
        }

        var biographyResult = Biography.Create("Nieuw bij Nodo en klaar om te chatten.");
        if (!biographyResult.IsSuccess)
        {
            return Result.Error(biographyResult.Errors.ToArray());
        }

        var avatarResult = AvatarUrl.Create("https://ui-avatars.com/api/?name=Nodo+User&background=0B6532&color=fff");
        if (!avatarResult.IsSuccess)
        {
            return Result.Error(avatarResult.Errors.ToArray());
        }

        var userProfile = new User()
        {
            AccountId = registration.AccountId,
            FirstName = firstNameResult.Value,
            LastName = lastNameResult.Value,
            Biography = biographyResult.Value,
            AvatarUrl = avatarResult.Value,
            BirthDay = DateOnly.FromDateTime(DateTime.Today.AddYears(-18)),
            Gender = GenderType.X,
            OrganizationId = registration.OrganizationId,
            Organization = registration.Organization,
            SupervisorId = registration.AssignedSupervisorId,
            Supervisor = registration.AssignedSupervisor,
            UserSettings = new UserSetting()
            {
                FontSize = FontSize.Create(12),
                IsDarkMode = false,
            }
        };

        userProfile.UserSettings.AddChatTextLine("Hallo! Ik ben net gestart op Nodo.");

        _dbContext.Users.Add(userProfile);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
