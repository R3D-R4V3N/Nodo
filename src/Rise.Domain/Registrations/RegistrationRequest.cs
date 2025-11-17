using System;
using Ardalis.Result;
using Rise.Domain.Common;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using Rise.Domain.Users.Properties;

namespace Rise.Domain.Registrations;

public class RegistrationRequest : Entity
{
    private RegistrationRequest() { }

    public string Email { get; private set; } = string.Empty;
    public string NormalizedEmail { get; private set; } = string.Empty;
    public string FullName { get; private set; } = string.Empty;
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public DateOnly BirthDate { get; private set; }
        = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-18));
    public GenderType Gender { get; private set; } = GenderType.X;
    public string AvatarUrl { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public RegistrationStatus Status { get; private set; } = RegistrationStatus.Pending;

    public int OrganizationId { get; private set; }
    public Organization Organization { get; private set; } = null!;

    public int? AssignedSupervisorId { get; private set; }
    public Supervisor? AssignedSupervisor { get; private set; }

    public int? ApprovedBySupervisorId { get; private set; }
    public Supervisor? ApprovedBySupervisor { get; private set; }

    public DateTime? ApprovedAt { get; private set; }
    public string? DeniedReason { get; private set; }

    public static RegistrationRequest Create(
        string email,
        string normalizedEmail,
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
            Email = Guard.Against.NullOrWhiteSpace(email).Trim(),
            NormalizedEmail = Guard.Against.NullOrWhiteSpace(normalizedEmail).Trim(),
            FirstName = Guard.Against.NullOrWhiteSpace(firstName).Trim(),
            LastName = Guard.Against.NullOrWhiteSpace(lastName).Trim(),
            BirthDate = birthDate,
            Gender = gender,
            AvatarUrl = Guard.Against.NullOrWhiteSpace(avatarUrl).Trim(),
            PasswordHash = Guard.Against.NullOrWhiteSpace(passwordHash),
        };

        request.FullName = $"{request.FirstName} {request.LastName}".Trim();

        request.AssignOrganization(organization);
        return request;
    }

    public Result AssignSupervisor(Supervisor supervisor)
    {
        if (Status is not RegistrationStatus.Pending)
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        var target = Guard.Against.Null(supervisor);

        if (target.Organization.Id != OrganizationId)
        {
            return Result.Invalid(
                new ValidationError(nameof(AssignedSupervisorId), "Begeleider behoort niet tot dezelfde organisatie."));
        }

        AssignedSupervisor = target;
        AssignedSupervisorId = target.Id;

        return Result.Success();
    }

    public Result Approve(Supervisor approver, Supervisor assignedSupervisor)
    {
        if (Status is not RegistrationStatus.Pending)
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        var supervisor = Guard.Against.Null(approver);

        if (supervisor.Organization.Id != OrganizationId)
        {
            return Result.Unauthorized();
        }

        var assignResult = AssignSupervisor(assignedSupervisor);
        if (!assignResult.IsSuccess)
        {
            return assignResult;
        }

        ApprovedBySupervisor = supervisor;
        ApprovedBySupervisorId = supervisor.Id;
        ApprovedAt = DateTime.UtcNow;
        Status = RegistrationStatus.Approved;

        return Result.Success();
    }

    public Result Reject(Supervisor approver, string? reason = null)
    {
        if (Status is not RegistrationStatus.Pending)
        {
            return Result.Conflict("Aanvraag werd al verwerkt.");
        }

        var supervisor = Guard.Against.Null(approver);

        if (supervisor.Organization.Id != OrganizationId)
        {
            return Result.Unauthorized();
        }

        Status = RegistrationStatus.Rejected;
        ApprovedBySupervisor = supervisor;
        ApprovedBySupervisorId = supervisor.Id;
        ApprovedAt = DateTime.UtcNow;
        DeniedReason = reason?.Trim();

        return Result.Success();
    }

    private void AssignOrganization(Organization organization)
    {
        Organization = Guard.Against.Null(organization);
        OrganizationId = organization.Id;
    }
}
