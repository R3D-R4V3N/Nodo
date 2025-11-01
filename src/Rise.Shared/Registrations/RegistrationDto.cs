namespace Rise.Shared.Registrations;

public static class RegistrationDto
{
    public record PendingItem
    {
        public int Id { get; init; }
        public string AccountId { get; init; } = string.Empty;
        public string FirstName { get; init; } = string.Empty;
        public string LastName { get; init; } = string.Empty;
        public int OrganizationId { get; init; }
        public string OrganizationName { get; init; } = string.Empty;
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }
        public RegistrationStatus Status { get; init; }
    }

    public record Detail : PendingItem
    {
        public string? Feedback { get; init; }
        public SupervisorSummary? AssignedSupervisor { get; init; }
    }

    public record SupervisorSummary
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string AvatarUrl { get; init; } = string.Empty;
    }
}

public enum RegistrationStatus
{
    Pending,
    Approved,
    Rejected
}
