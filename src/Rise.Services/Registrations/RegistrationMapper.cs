using Rise.Domain.Users;
using Rise.Shared.Registrations;

using DomainRegistrationRequest = Rise.Domain.Registrations.RegistrationRequest;
using DomainRegistrationRequestStatus = Rise.Domain.Registrations.RegistrationRequestStatus;

namespace Rise.Services.Registrations;

internal static class RegistrationMapper
{
    internal static RegistrationDto.PendingItem ToPendingDto(this DomainRegistrationRequest registration) =>
        new()
        {
            Id = registration.Id,
            AccountId = registration.AccountId,
            FirstName = registration.FirstName.Value,
            LastName = registration.LastName.Value,
            OrganizationId = registration.OrganizationId,
            OrganizationName = registration.Organization?.Name ?? string.Empty,
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt,
            Status = registration.Status.ToDto()
        };

    internal static RegistrationDto.Detail ToDetailDto(this DomainRegistrationRequest registration)
    {
        var detail = new RegistrationDto.Detail
        {
            Id = registration.Id,
            AccountId = registration.AccountId,
            FirstName = registration.FirstName.Value,
            LastName = registration.LastName.Value,
            OrganizationId = registration.OrganizationId,
            OrganizationName = registration.Organization?.Name ?? string.Empty,
            CreatedAt = registration.CreatedAt,
            UpdatedAt = registration.UpdatedAt,
            Status = registration.Status.ToDto(),
            Feedback = registration.Feedback,
            AssignedSupervisor = registration.AssignedSupervisor.ToDto()
        };

        return detail;
    }

    private static RegistrationStatus ToDto(this DomainRegistrationRequestStatus status) => status switch
    {
        DomainRegistrationRequestStatus.Pending => RegistrationStatus.Pending,
        DomainRegistrationRequestStatus.Approved => RegistrationStatus.Approved,
        DomainRegistrationRequestStatus.Rejected => RegistrationStatus.Rejected,
        _ => RegistrationStatus.Pending
    };

    private static RegistrationDto.SupervisorSummary? ToDto(this Supervisor? supervisor)
    {
        if (supervisor is null)
        {
            return null;
        }

        return new RegistrationDto.SupervisorSummary
        {
            Id = supervisor.Id,
            Name = $"{supervisor.FirstName} {supervisor.LastName}",
            AvatarUrl = supervisor.AvatarUrl
        };
    }
}
