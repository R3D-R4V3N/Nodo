using Ardalis.GuardClauses;
using Ardalis.Result;
using Rise.Domain.Common;
using Rise.Domain.Organizations;
using Rise.Domain.Users;
using System;

namespace Rise.Domain.Users.Registrations;

public class UserRegistrationRequest : Entity
{
    public const int MaxNoteLength = 500;

    public string AccountId { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? FullName { get; private set; }

    public RegistrationStatus Status { get; private set; } = RegistrationStatus.Pending;

    public DateTime? ProcessedAt { get; private set; }

    public string? DecisionNote { get; private set; }

    public int OrganizationId { get; private set; }

    public Organization Organization { get; private set; } = null!;

    public int? AssignedSupervisorId { get; private set; }

    public Supervisor? AssignedSupervisor { get; private set; }

    public int? ProcessedBySupervisorId { get; private set; }

    public Supervisor? ProcessedBySupervisor { get; private set; }

    public bool IsPending => Status == RegistrationStatus.Pending;

    public static Result<UserRegistrationRequest> Create(
        string accountId,
        string email,
        Organization organization,
        string? fullName = null)
    {
        Guard.Against.Null(organization);

        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result.Invalid(new ValidationError(nameof(AccountId), "AccountId is vereist."));
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Invalid(new ValidationError(nameof(Email), "E-mailadres is vereist."));
        }

        var request = new UserRegistrationRequest
        {
            AccountId = accountId,
            Email = email.Trim(),
            Organization = organization,
            OrganizationId = organization.Id,
            FullName = string.IsNullOrWhiteSpace(fullName) ? null : fullName.Trim(),
        };

        organization.AddRegistrationRequest(request);

        return Result.Success(request);
    }

    public Result AssignSupervisor(Supervisor supervisor)
    {
        Guard.Against.Null(supervisor);

        if (!IsPending)
        {
            return Result.Conflict("De aanvraag is al verwerkt.");
        }

        if (supervisor.OrganizationId != OrganizationId)
        {
            return Result.Conflict("Begeleider behoort niet tot dezelfde organisatie.");
        }

        AssignedSupervisor = supervisor;
        AssignedSupervisorId = supervisor.Id;

        return Result.Success();
    }

    public Result Approve(Supervisor approver, string? note = null)
    {
        Guard.Against.Null(approver);

        if (!IsPending)
        {
            return Result.Conflict("De aanvraag is al verwerkt.");
        }

        if (approver.OrganizationId != OrganizationId)
        {
            return Result.Conflict("Begeleider behoort niet tot dezelfde organisatie.");
        }

        if (AssignedSupervisor is null)
        {
            return Result.Conflict("Koppel eerst een begeleider voordat je de aanvraag goedkeurt.");
        }

        Status = RegistrationStatus.Approved;
        ProcessedAt = DateTime.UtcNow;
        ProcessedBySupervisor = approver;
        ProcessedBySupervisorId = approver.Id;
        DecisionNote = TrimNote(note);

        return Result.Success();
    }

    public Result Reject(Supervisor approver, string? note = null)
    {
        Guard.Against.Null(approver);

        if (!IsPending)
        {
            return Result.Conflict("De aanvraag is al verwerkt.");
        }

        if (approver.OrganizationId != OrganizationId)
        {
            return Result.Conflict("Begeleider behoort niet tot dezelfde organisatie.");
        }

        Status = RegistrationStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
        ProcessedBySupervisor = approver;
        ProcessedBySupervisorId = approver.Id;
        DecisionNote = TrimNote(note);

        return Result.Success();
    }

    private static string? TrimNote(string? note)
    {
        if (string.IsNullOrWhiteSpace(note))
        {
            return null;
        }

        var trimmed = note.Trim();
        return trimmed.Length <= MaxNoteLength ? trimmed : trimmed[..MaxNoteLength];
    }
}
