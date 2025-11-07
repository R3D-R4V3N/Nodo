using Rise.Domain.Organizations;
using Rise.Shared.Registrations;

namespace Rise.Services.Registrations;

internal static class RegistrationMapper
{
    public static RegistrationDto.Pending ToPendingDto(UserRegistration registration)
    {
        return new RegistrationDto.Pending
        {
            Id = registration.Id,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            Email = registration.Email,
            OrganizationName = registration.Organization.Name,
            AssignedSupervisorName = registration.AssignedSupervisor is null
                ? null
                : $"{registration.AssignedSupervisor.FirstName.Value} {registration.AssignedSupervisor.LastName.Value}",
            AssignedSupervisorAccountId = registration.AssignedSupervisor?.AccountId,
            RequestedAt = registration.RequestedAt,
        };
    }
}
