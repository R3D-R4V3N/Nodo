using Ardalis.Result;
using Microsoft.EntityFrameworkCore;
using Rise.Domain.Users;
using Rise.Persistence;
using Rise.Services.Identity;
using Rise.Shared.Identity;
using Rise.Shared.Registrations;

using DomainRegistrationRequestStatus = Rise.Domain.Registrations.RegistrationRequestStatus;
using SharedRegistrationRequest = Rise.Shared.Registrations.RegistrationRequest;

namespace Rise.Services.Registrations;

public class RegistrationService(
    ApplicationDbContext dbContext,
    ISessionContextProvider sessionContextProvider) : IRegistrationService
{
    private readonly ApplicationDbContext _dbContext = dbContext;
    private readonly ISessionContextProvider _sessionContextProvider = sessionContextProvider;

    public async Task<Result<RegistrationResponse.PendingList>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        var supervisorResult = await GetCurrentSupervisorAsync(cancellationToken);
        if (!supervisorResult.IsSuccess)
        {
            return ConvertFailure<RegistrationResponse.PendingList>(supervisorResult);
        }

        var supervisor = supervisorResult.Value;

        var registrations = await _dbContext.RegistrationRequests
            .AsNoTracking()
            .Include(r => r.Organization)
            .Where(r => r.Status == DomainRegistrationRequestStatus.Pending && r.OrganizationId == supervisor.OrganizationId)
            .OrderBy(r => r.CreatedAt)
            .ToListAsync(cancellationToken);

        return Result.Success(new RegistrationResponse.PendingList
        {
            Registrations = registrations.Select(r => r.ToPendingDto()).ToList()
        });
    }

    public async Task<Result<RegistrationResponse.Detail>> GetDetailAsync(int registrationId, CancellationToken cancellationToken = default)
    {
        var supervisorResult = await GetCurrentSupervisorAsync(cancellationToken);
        if (!supervisorResult.IsSuccess)
        {
            return ConvertFailure<RegistrationResponse.Detail>(supervisorResult);
        }

        var supervisor = supervisorResult.Value;

        var registration = await _dbContext.RegistrationRequests
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .SingleOrDefaultAsync(r => r.Id == registrationId, cancellationToken);

        if (registration is null)
        {
            return Result<RegistrationResponse.Detail>.NotFound();
        }

        if (registration.OrganizationId != supervisor.OrganizationId)
        {
            return Result<RegistrationResponse.Detail>.Forbidden();
        }

        return Result.Success(new RegistrationResponse.Detail
        {
            Registration = registration.ToDetailDto()
        });
    }

    public async Task<Result> ApproveAsync(SharedRegistrationRequest.Approve request, CancellationToken cancellationToken = default)
    {
        var supervisorResult = await GetCurrentSupervisorAsync(cancellationToken);
        if (!supervisorResult.IsSuccess)
        {
            return ConvertFailure(supervisorResult);
        }

        var supervisor = supervisorResult.Value;

        var registration = await _dbContext.RegistrationRequests
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .SingleOrDefaultAsync(r => r.Id == request.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return Result.NotFound();
        }

        if (registration.OrganizationId != supervisor.OrganizationId)
        {
            return Result.Forbidden();
        }

        if (registration.Status != DomainRegistrationRequestStatus.Pending)
        {
            return Result.Conflict("Aanvraag is al verwerkt.");
        }

        var assignedSupervisor = await GetSupervisorByIdAsync(request.SupervisorId, cancellationToken);
        if (!assignedSupervisor.IsSuccess)
        {
            return ConvertFailure(assignedSupervisor);
        }

        if (assignedSupervisor.Value.OrganizationId != registration.OrganizationId)
        {
            return Result.Forbidden("Supervisor behoort niet tot dezelfde organisatie.");
        }

        registration.Approve(assignedSupervisor.Value, request.Feedback);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> RejectAsync(SharedRegistrationRequest.Reject request, CancellationToken cancellationToken = default)
    {
        var supervisorResult = await GetCurrentSupervisorAsync(cancellationToken);
        if (!supervisorResult.IsSuccess)
        {
            return ConvertFailure(supervisorResult);
        }

        var supervisor = supervisorResult.Value;

        var registration = await _dbContext.RegistrationRequests
            .Include(r => r.Organization)
            .Include(r => r.AssignedSupervisor)
            .SingleOrDefaultAsync(r => r.Id == request.RegistrationId, cancellationToken);

        if (registration is null)
        {
            return Result.NotFound();
        }

        if (registration.OrganizationId != supervisor.OrganizationId)
        {
            return Result.Forbidden();
        }

        if (registration.Status != DomainRegistrationRequestStatus.Pending)
        {
            return Result.Conflict("Aanvraag is al verwerkt.");
        }

        var assignedSupervisor = await GetSupervisorByIdAsync(request.SupervisorId, cancellationToken);
        if (!assignedSupervisor.IsSuccess)
        {
            return ConvertFailure(assignedSupervisor);
        }

        if (assignedSupervisor.Value.OrganizationId != registration.OrganizationId)
        {
            return Result.Forbidden("Supervisor behoort niet tot dezelfde organisatie.");
        }

        registration.Reject(assignedSupervisor.Value, request.Feedback);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<Supervisor>> GetCurrentSupervisorAsync(CancellationToken cancellationToken)
    {
        var principal = _sessionContextProvider.User;
        if (principal is null)
        {
            return Result.Unauthorized();
        }

        if (!principal.IsInRole(AppRoles.Supervisor))
        {
            return Result.Forbidden();
        }

        var accountId = principal.GetUserId();
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Unauthorized();
        }

        var supervisor = await _dbContext.Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.AccountId == accountId, cancellationToken);

        if (supervisor is null)
        {
            return Result.Unauthorized("De huidige gebruiker is geen supervisor.");
        }

        return Result.Success(supervisor);
    }

    private async Task<Result<Supervisor>> GetSupervisorByIdAsync(int supervisorId, CancellationToken cancellationToken)
    {
        var supervisor = await _dbContext.Supervisors
            .Include(s => s.Organization)
            .SingleOrDefaultAsync(s => s.Id == supervisorId, cancellationToken);

        return supervisor is null
            ? Result.NotFound("Supervisor niet gevonden.")
            : Result.Success(supervisor);
    }

    private static Result<T> ConvertFailure<T>(Result failure)
    {
        return failure.Status switch
        {
            ResultStatus.Forbidden => Result<T>.Forbidden(failure.Errors.ToArray()),
            ResultStatus.Unauthorized => Result<T>.Unauthorized(failure.Errors.ToArray()),
            ResultStatus.NotFound => Result<T>.NotFound(failure.Errors.ToArray()),
            ResultStatus.Conflict => Result<T>.Conflict(failure.Errors.ToArray()),
            ResultStatus.CriticalError => Result<T>.CriticalError(failure.Errors.ToArray()),
            ResultStatus.Invalid => Result<T>.Invalid(failure.ValidationErrors),
            ResultStatus.Ok => Result<T>.Success(),
            _ => Result<T>.Error(failure.Errors.ToArray())
        };
    }

    private static Result ConvertFailure(Result failure)
    {
        return failure.Status switch
        {
            ResultStatus.Forbidden => Result.Forbidden(failure.Errors.ToArray()),
            ResultStatus.Unauthorized => Result.Unauthorized(failure.Errors.ToArray()),
            ResultStatus.NotFound => Result.NotFound(failure.Errors.ToArray()),
            ResultStatus.Conflict => Result.Conflict(failure.Errors.ToArray()),
            ResultStatus.CriticalError => Result.CriticalError(failure.Errors.ToArray()),
            ResultStatus.Invalid => Result.Invalid(failure.ValidationErrors),
            ResultStatus.Ok => Result.Success(),
            _ => Result.Error(failure.Errors.ToArray())
        };
    }
}
