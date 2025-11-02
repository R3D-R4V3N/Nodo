using System;
using System.Collections.Generic;

namespace Rise.Shared.Registrations;

public static class RegistrationRequestResponse
{
    public record PendingItem
    {
        public int Id { get; init; }
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int OrganizationId { get; init; }
        public string OrganizationName { get; init; } = string.Empty;
        public DateTime RequestedAt { get; init; }
        public IReadOnlyList<SupervisorListItem> Supervisors { get; init; } = Array.Empty<SupervisorListItem>();
    }

    public record SupervisorListItem(int Id, string Name);
}
