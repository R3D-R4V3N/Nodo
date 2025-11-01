using Ardalis.GuardClauses;
using Rise.Domain.Common;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;

namespace Rise.Domain.Registrations;

public class RegistrationRequest : Entity
{
    // EF
    public RegistrationRequest() { }

    private string _accountId = default!;
    public required string AccountId
    {
        get => _accountId;
        init => _accountId = Guard.Against.NullOrWhiteSpace(value);
    }

    private FirstName _firstName = default!;
    public required FirstName FirstName
    {
        get => _firstName;
        set => _firstName = Guard.Against.Null(value);
    }

    private LastName _lastName = default!;
    public required LastName LastName
    {
        get => _lastName;
        set => _lastName = Guard.Against.Null(value);
    }

    private Organization _organization = default!;
    public required Organization Organization
    {
        get => _organization;
        set => _organization = Guard.Against.Null(value);
    }

    public int OrganizationId { get; set; }

    public RegistrationRequestStatus Status { get; private set; } = RegistrationRequestStatus.Pending;

    public int? AssignedSupervisorId { get; private set; }
    public Supervisor? AssignedSupervisor { get; private set; }

    public void Approve(Supervisor supervisor)
    {
        Guard.Against.Null(supervisor);
        Status = RegistrationRequestStatus.Approved;
        AssignedSupervisor = supervisor;
        AssignedSupervisorId = supervisor.Id;
    }

    public void Reject(Supervisor supervisor)
    {
        Guard.Against.Null(supervisor);
        Status = RegistrationRequestStatus.Rejected;
        AssignedSupervisor = supervisor;
        AssignedSupervisorId = supervisor.Id;
    }

    public void Reset()
    {
        Status = RegistrationRequestStatus.Pending;
        AssignedSupervisor = null;
        AssignedSupervisorId = null;
    }
}
