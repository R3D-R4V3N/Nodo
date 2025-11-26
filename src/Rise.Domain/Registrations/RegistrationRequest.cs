using Ardalis.Result;
using Rise.Domain.Common.ValueObjects;
using Rise.Domain.Organizations;
using Rise.Domain.Users;

namespace Rise.Domain.Registrations;

public partial class RegistrationRequest : Entity
{
    private RegistrationRequest() { }

    public Email Email { get; set; }
    public FirstName FirstName { get; set; }
    public LastName LastName { get; set; }
    public BirthDay BirthDay { get; set; }
    public GenderType Gender { get; set; }
    public AvatarUrl AvatarUrl { get; set; }
    public string PasswordHash { get; set; }
    
    // organization
    public Organization Organization { get; set; }

    // supervisor
    public Supervisor? AssignedSupervisor { get; set; }

    // registration status
    private RegistrationStatus _status;
    public required RegistrationStatus Status
    {
        get => _status;
        set
        {
            if (_status == value) return;

            _status = Guard.Against.Null(value);
            if (_status.Request != this)
            {
                _status.Request = this;
            }
        }
    }

    public static RegistrationRequest Create(
        string email,
        string firstName,
        string lastName,
        DateOnly birthDate,
        GenderType gender,
        string avatarUrl,
        string passwordHash,
        Organization organization)
    {
        var request = new RegistrationRequest
        {
            Email = Email.Create(email),
            FirstName = FirstName.Create(firstName),
            LastName = LastName.Create(lastName),
            BirthDay = BirthDay.Create(birthDate),
            Gender = gender,
            AvatarUrl = AvatarUrl.Create(avatarUrl),
            PasswordHash = Guard.Against.NullOrWhiteSpace(passwordHash),
            Organization = Guard.Against.Null(organization),
            Status = new RegistrationStatus()
            {
                StatusType = RegistrationStatusType.Pending,
            }
        };

        return request;
    }

    public Result AssignSupervisor(Supervisor supervisor)
    {
        if (Status is not { StatusType: RegistrationStatusType.Pending })
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        supervisor = Guard.Against.Null(supervisor);

        if (supervisor.Organization.Id != Organization.Id)
        {
            return Result.Invalid(
                new ValidationError(nameof(supervisor), "Begeleider behoort niet tot dezelfde organisatie."));
        }

        AssignedSupervisor = supervisor;

        return Result.Success();
    }

    public Result Approve(Supervisor approver, Supervisor assignedSupervisor)
    {
        if (Status is not { StatusType: RegistrationStatusType.Pending })
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        approver = Guard.Against.Null(approver);

        if (approver.Organization.Id != Organization.Id)
        {
            return Result.Unauthorized();
        }

        var assignResult = AssignSupervisor(assignedSupervisor);
        if (!assignResult.IsSuccess)
        {
            return assignResult;
        }

        Status = new RegistrationStatus()
        {
            HandledBy = approver,
            HandledDate = DateTime.UtcNow,
            StatusType = RegistrationStatusType.Approved,
        };

        return Result.Success();
    }

    public Result Reject(Supervisor approver, string? reason = null)
    {
        if (Status is not { StatusType: RegistrationStatusType.Pending })
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        approver = Guard.Against.Null(approver);

        if (approver.Organization.Id != Organization.Id)
        {
            return Result.Unauthorized();
        }

        Status = new RegistrationStatus()
        {
            HandledBy = approver,
            HandledDate = DateTime.UtcNow,
            StatusType = RegistrationStatusType.Rejected,
            Note = string.IsNullOrWhiteSpace(reason) ? null : RegistrationNote.Create(reason)
        };

        return Result.Success();
    }
}
