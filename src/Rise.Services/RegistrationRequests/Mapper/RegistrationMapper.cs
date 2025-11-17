using Rise.Domain.Organizations;
using Rise.Domain.Registrations;
using Rise.Domain.Users;
using Rise.Domain.Users.Connections;
using Rise.Domain.Users.Properties;
using Rise.Services.Users.Mapper;
using Rise.Shared.RegistrationRequests;
using Rise.Shared.UserConnections;
using Rise.Shared.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            FullName = registration.FullName,
            FirstName = registration.FirstName,
            LastName = registration.LastName,
            BirthDate = registration.BirthDate,
            Gender = registration.Gender.ToDto(),
            AvatarUrl = registration.AvatarUrl,
            OrganizationId = registration.OrganizationId,
            OrganizationName = registration.Organization?.Name ?? string.Empty,
            SubmittedAt = registration.CreatedAt,
            AssignedSupervisorId = registration.AssignedSupervisorId,
            Supervisors = supervisors
        };
}
