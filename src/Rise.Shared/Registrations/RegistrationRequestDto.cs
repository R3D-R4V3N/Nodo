using System;

namespace Rise.Shared.Registrations;

public static class RegistrationRequestDto
{
    public record ListItem
    {
        public required int Id { get; init; }
        public required string AccountId { get; init; }
        public required string Email { get; init; }
        public string? FullName { get; init; }
        public required int OrganizationId { get; init; }
        public required string OrganizationName { get; init; }
        public int? AssignedSupervisorId { get; init; }
        public string? AssignedSupervisorName { get; init; }
        public RegistrationStatusDto Status { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}
