namespace Rise.Shared.RegistrationRequests;

public static class RegistrationRequestDto
{
    public record Pending
    {
        public int Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string OrganizationName { get; init; } = string.Empty;
        public int OrganizationId { get; init; }
        public DateTime SubmittedAt { get; init; }
        public int? AssignedSupervisorId { get; init; }
        public IReadOnlyList<SupervisorOption> Supervisors { get; init; } = [];
    }

    public record SupervisorOption
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
    }
}
