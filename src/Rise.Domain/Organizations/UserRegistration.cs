using Ardalis.Result;
using Rise.Domain.Common;
using Rise.Domain.Users;

namespace Rise.Domain.Organizations;

public class UserRegistration : Entity
{
    public required string AccountId { get; init; }
    public required string Email { get; init; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public int? AssignedSupervisorId { get; private set; }
    public Supervisor? AssignedSupervisor { get; private set; }

    public RegistrationStatus Status { get; private set; } = RegistrationStatus.Pending;
    public DateTime RequestedAt { get; init; } = DateTime.UtcNow;
    public DateTime? ApprovedAt { get; private set; }

    public Result AssignSupervisor(Supervisor supervisor)
    {
        if (supervisor.OrganizationId != OrganizationId)
        {
            return Result.Conflict("Begeleider behoort niet tot dezelfde organisatie.");
        }

        AssignedSupervisor = supervisor;
        AssignedSupervisorId = supervisor.Id;
        return Result.Success();
    }

    public Result Approve()
    {
        if (Status != RegistrationStatus.Pending)
        {
            return Result.Conflict("De registratie is al verwerkt.");
        }

        if (AssignedSupervisorId is null)
        {
            return Result.Conflict("Koppel eerst een begeleider aan de aanvraag.");
        }

        Status = RegistrationStatus.Approved;
        ApprovedAt = DateTime.UtcNow;
        return Result.Success();
    }
}
