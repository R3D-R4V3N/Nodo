using Rise.Domain.Registrations;
using Rise.Domain.Users;
using Rise.Services.Users.Mapper;
using Rise.Shared.RegistrationRequests;

namespace Rise.Services.RegistrationRequests.Mapper;

public static class RegistrationMapper
{
    public static RegistrationRequestDto.SupervisorOption ToSupervisorOption(this Supervisor supervisor) =>
        new RegistrationRequestDto.SupervisorOption
        {
            Id = supervisor.Id,
            Name = $"{supervisor.FirstName.Value} {supervisor.LastName.Value}",
        };
    public static RegistrationRequestDto.Pending ToPending(this RegistrationRequest registration, List<RegistrationRequestDto.SupervisorOption> supervisors) =>
        new RegistrationRequestDto.Pending
        {
            Id = registration.Id,
            Email = registration.Email,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            BirthDate = registration.BirthDay,
            Gender = registration.Gender.ToDto(),
            AvatarUrl = registration.AvatarUrl,
            OrganizationName = registration.Organization?.Name ?? string.Empty,
            SubmittedAt = registration.CreatedAt,
            Supervisors = supervisors
        };
}
